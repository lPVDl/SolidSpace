using System.Collections.Generic;
using SolidSpace.GameCycle;
using SolidSpace.UI;

namespace SolidSpace.Playground.Core
{
    internal class PlaygroundCoreController : IController
    {
        public EControllerType ControllerType => EControllerType.Playground;
        
        private readonly PlaygroundCoreConfig _config;
        private readonly IUIManager _uiManager;
        private readonly List<IPlaygroundTool> _tools;

        private IToolWindowView _window;
        private IToolButtonView[] _buttons;
        private int _toolIndex;

        public PlaygroundCoreController(PlaygroundCoreConfig config, IUIManager uiManager, List<IPlaygroundTool> tools)
        {
            _config = config;
            _uiManager = uiManager;
            _tools = tools;
        }
        
        public void InitializeController()
        {
            _window = _uiManager.Instantiate(_config.ToolWindowPrefab);
            _uiManager.AttachToRoot(_window, "RootLeft");

            _toolIndex = -1;
            _buttons = new IToolButtonView[_tools.Count];

            for (var i = 0; i < _tools.Count; i++)
            {
                var toolView = _uiManager.Instantiate(_config.CheckedButtonPrefab);
                _window.AttachChild(toolView);
                _buttons[i] = toolView;
                var toolIndex = i;
                toolView.OnClick += () => OnToolViewClicked(toolIndex);
                toolView.SetSelected(false);
                var tool = _tools[i];
                tool.InitializeTool();
                toolView.SetIcon(tool.Config.Icon);
            }
        }
        
        public void UpdateController()
        {
            if (_toolIndex != -1)
            {
                _tools[_toolIndex].Update();
            }
        }

        private void OnToolViewClicked(int newIndex)
        {
            if (_toolIndex == newIndex)
            {
                _buttons[_toolIndex].SetSelected(false);
                _tools[_toolIndex].OnToolDeactivation();
                _toolIndex = -1;
                return;
            }

            if (_toolIndex != -1)
            {
                _buttons[_toolIndex].SetSelected(false);
                _tools[_toolIndex].OnToolDeactivation();
            }

            _toolIndex = newIndex;
            _buttons[_toolIndex].SetSelected(true);
            _tools[_toolIndex].OnToolActivation();
        }

        public void FinalizeController()
        {
            foreach (var tool in _tools)
            {
                tool.FinalizeTool();
            }
        }
    }
}