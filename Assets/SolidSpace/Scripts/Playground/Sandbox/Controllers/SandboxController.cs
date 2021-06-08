using System;
using System.Linq;
using SolidSpace.GameCycle;
using SolidSpace.Playground.UI;

namespace SolidSpace.Playground.Sandbox
{
    internal class SandboxController : IController
    {
        public EControllerType ControllerType => EControllerType.Playground;
        
        private readonly SandboxConfig _config;
        private readonly IUIManager _uiManager;
        
        private IToolWindowView _windowView;
        private IToolButtonView[] _toolViews;
        private int _selectedTool;

        public SandboxController(SandboxConfig config, IUIManager uiManager)
        {
            _config = config;
            _uiManager = uiManager;
        }
        
        public void InitializeController()
        {
            _windowView = _uiManager.Instantiate(_config.ToolWindowPrefab);
            _uiManager.AttachToRoot(_windowView, "RootLeft");

            var toolTypes = (EPlaygroundTool[]) Enum.GetValues(typeof(EPlaygroundTool));
            _toolViews = new IToolButtonView[toolTypes.Length];
            _selectedTool = -1;

            var toolIcons = _config.ToolIcons.ToDictionary(i => i.tool, i => i.icon);
            
            for (var i = 0; i < toolTypes.Length; i++)
            {
                var toolView = _uiManager.Instantiate(_config.CheckedButtonPrefab);
                _windowView.AttachChild(toolView);
                _toolViews[i] = toolView;
                var toolIndex = i;
                toolView.OnClick += () => OnToolViewClicked(toolIndex);
                toolView.SetSelected(false);
                var toolType = toolTypes[i];
                toolView.SetIcon(toolIcons[toolType]);
            }
        }
        
        public void UpdateController()
        {
            
        }

        private void OnToolViewClicked(int index)
        {
            if (_selectedTool == index)
            {
                _toolViews[_selectedTool].SetSelected(false);
                _selectedTool = -1;
                return;
            }

            if (_selectedTool != -1)
            {
                _toolViews[_selectedTool].SetSelected(false);
            }

            _selectedTool = index;
            _toolViews[_selectedTool].SetSelected(true);
        }

        public void FinalizeController()
        {
            
        }
    }
}