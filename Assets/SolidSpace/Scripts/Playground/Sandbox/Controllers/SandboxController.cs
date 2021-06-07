using SolidSpace.GameCycle;
using SolidSpace.Playground.Sandbox.Data;
using SolidSpace.Playground.UI;

namespace SolidSpace.Playground.Sandbox
{
    internal class SandboxController : IController
    {
        public EControllerType ControllerType => EControllerType.Playground;
        
        private readonly SandboxConfig _config;
        private readonly IUIManager _uiManager;

        private UIElementHandle _view;

        public SandboxController(SandboxConfig config, IUIManager uiManager)
        {
            _config = config;
            _uiManager = uiManager;
        }
        
        public void InitializeController()
        {
            _view = _uiManager.CreateElement(_config.UIPrefab);
            _uiManager.AttachElementToRoot(_view, "RootLeft");
        }

        public void UpdateController()
        {
            
        }

        public void FinalizeController()
        {
            
        }
    }
}