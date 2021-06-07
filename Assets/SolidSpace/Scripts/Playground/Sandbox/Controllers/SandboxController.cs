using SolidSpace.GameCycle;
using SolidSpace.Playground.Sandbox.Data;
using SolidSpace.Playground.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Sandbox
{
    internal class SandboxController : IController
    {
        public EControllerType ControllerType => EControllerType.Playground;
        
        private readonly SandboxConfig _config;
        private readonly IUIManager _uiManager;

        private UIElementHandle _view;

        private bool _isActive;
        private ICheckedButton _button;

        public SandboxController(SandboxConfig config, IUIManager uiManager)
        {
            _config = config;
            _uiManager = uiManager;
        }
        
        public void InitializeController()
        {
            _button = _uiManager.Instantiate(_config.CheckedButtonPrefab);
            _uiManager.AttachToRoot(_button, "RootLeft");

            _isActive = false;
            _button.SetChecked(true);

            _button.OnClick += OnButtonClicked;

            // _view = _uiManager.CreateElement(_config.ToolWindowPrefab);
            // _uiManager.AttachElementToRoot(_view, "RootLeft");
            //
            // var button = _uiManager.CreateElement<CheckedButtonView>(_config.ToolIconPrefab);
            //
            // _uiManager.AttachElementToRoot(button.Handle, "RootLeft");

            // button.OnClick += () => button.SetChecked(false);
        }

        private void OnButtonClicked()
        {
            _isActive = !_isActive;
            _button.SetChecked(_isActive);
        }

        private void Callback(MouseDownEvent evt)
        {
            Debug.LogError("MOUSE DOWN!!!");
        }

        public void UpdateController()
        {
            
        }

        public void FinalizeController()
        {
            
        }
    }
}