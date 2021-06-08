using SolidSpace.GameCycle;
using SolidSpace.Playground.UI;

namespace SolidSpace.Playground.Sandbox
{
    internal class SandboxController : IController
    {
        public EControllerType ControllerType => EControllerType.Playground;
        
        private readonly SandboxConfig _config;
        private readonly IUIManager _uiManager;

        private bool _isActive;
        private ICheckedButtonView _button;

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
        }

        private void OnButtonClicked()
        {
            _isActive = !_isActive;
            _button.SetChecked(_isActive);
        }

        public void UpdateController()
        {
            
        }

        public void FinalizeController()
        {
            
        }
    }
}