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

        public SandboxController(SandboxConfig config, IUIManager uiManager)
        {
            _config = config;
            _uiManager = uiManager;
        }
        
        public void InitializeController()
        {
            _windowView = _uiManager.Instantiate(_config.ToolWindowPrefab);
            _uiManager.AttachToRoot(_windowView, "RootLeft");

            for (var i = 0; i < 3; i++)
            {
                var buttonView = _uiManager.Instantiate(_config.CheckedButtonPrefab);
                _windowView.AttachChild(buttonView);
            }
        }

        public void UpdateController()
        {
            
        }

        public void FinalizeController()
        {
            
        }
    }
}