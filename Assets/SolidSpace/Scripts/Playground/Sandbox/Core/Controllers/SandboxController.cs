using System.Collections.Generic;
using SolidSpace.GameCycle;
using SolidSpace.Playground.Sandbox.Core;
using SolidSpace.Playground.UI;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox
{
    internal class SandboxController : IController
    {
        public EControllerType ControllerType => EControllerType.Playground;
        
        private readonly SandboxConfig _config;
        private readonly IUIManager _uiManager;
        private readonly List<IPlaygroundTool> _tools;
        private readonly Camera _camera;

        private IToolWindowView _window;
        private IToolButtonView[] _buttons;
        private MouseClickTool _mouseClickTool;
        private int _toolIndex;

        public SandboxController(SandboxConfig config, IUIManager uiManager, List<IPlaygroundTool> tools, Camera camera)
        {
            _config = config;
            _uiManager = uiManager;
            _tools = tools;
            _camera = camera;
        }
        
        public void InitializeController()
        {
            _mouseClickTool = new MouseClickTool(_uiManager, _camera);
            
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
                tool.Initialize();
                toolView.SetIcon(tool.Icon);
            }
        }
        
        public void UpdateController()
        {
            if (_toolIndex == -1)
            {
                return;
            }

            if (!_mouseClickTool.CheckMouseClick(out var clickPosition))
            {
                return;
            }
            
            _tools[_toolIndex].OnMouseClick(clickPosition);
        }

        private void OnToolViewClicked(int newIndex)
        {
            if (_toolIndex == newIndex)
            {
                _buttons[_toolIndex].SetSelected(false);
                _tools[_toolIndex].OnToolDeselected();
                _toolIndex = -1;
                return;
            }

            if (_toolIndex != -1)
            {
                _buttons[_toolIndex].SetSelected(false);
                _tools[_toolIndex].OnToolDeselected();
            }

            _toolIndex = newIndex;
            _buttons[_toolIndex].SetSelected(true);
            _tools[_toolIndex].OnToolSelected();
        }

        public void FinalizeController()
        {
            foreach (var tool in _tools)
            {
                
            }
        }
    }
}