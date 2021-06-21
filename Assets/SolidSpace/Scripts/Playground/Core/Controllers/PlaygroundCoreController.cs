using System.Collections.Generic;
using SolidSpace.GameCycle;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Core
{
    internal class PlaygroundCoreController : IInitializable, IUpdatable
    {
        private const string NoToolTitle = "Tools";
        
        private readonly IUIManager _uiManager;
        private readonly List<IPlaygroundTool> _tools;
        private readonly IPlaygroundUIFactory _uiFactory;

        private IToolWindow _window;
        private IToolButton[] _buttons;
        private int _toolIndex;
        private PlaygroundToolConfig[] _configs;

        public PlaygroundCoreController(IUIManager uiManager, List<IPlaygroundTool> tools, IPlaygroundUIFactory uiFactory)
        {
            _uiManager = uiManager;
            _tools = tools;
            _uiFactory = uiFactory;
        }
        
        public void Initialize()
        {
            _window = _uiFactory.CreateToolWindow();
            _uiManager.AddToRoot(_window, "ContainerA");
            _window.SetTitle(NoToolTitle);

            var grid = _uiFactory.CreateLayoutGrid();
            grid.SetFlexDirection(FlexDirection.Row);
            _window.AttachChild(grid);

            _toolIndex = -1;
            _buttons = new IToolButton[_tools.Count];
            _configs = new PlaygroundToolConfig[_tools.Count];

            for (var i = 0; i < _tools.Count; i++)
            {
                var tool = _tools[i];
                tool.OnInitialize();
                
                var config = tool.Config;
                var button = _uiFactory.CreateToolButton();
                grid.AttachChild(button);
                var toolIndex = i;
                button.Clicked += () => OnToolViewClicked(toolIndex);
                button.SetSelected(false);
                button.SetIcon(config.Icon);
                
                _buttons[i] = button;
                _configs[i] = config;
            }
        }
        
        public void Update()
        {
            if (_toolIndex != -1)
            {
                _tools[_toolIndex].OnUpdate();
            }
        }

        private void OnToolViewClicked(int newIndex)
        {
            if (_toolIndex == newIndex)
            {
                _buttons[_toolIndex].SetSelected(false);
                _tools[_toolIndex].OnActivate(false);
                _toolIndex = -1;
                _window.SetTitle(NoToolTitle);
                return;
            }

            if (_toolIndex != -1)
            {
                _buttons[_toolIndex].SetSelected(false);
                _tools[_toolIndex].OnActivate(false);
            }

            _toolIndex = newIndex;
            _buttons[_toolIndex].SetSelected(true);
            _tools[_toolIndex].OnActivate(true);
            _window.SetTitle(_configs[_toolIndex].Name);
        }

        public void Finalize()
        {
            foreach (var tool in _tools)
            {
                tool.OnFinalize();
            }
        }
    }
}