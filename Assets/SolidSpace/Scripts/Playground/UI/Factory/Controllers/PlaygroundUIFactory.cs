using SolidSpace.Playground.UI.Elements;
using SolidSpace.UI;

namespace SolidSpace.Playground.UI.Factory
{
    internal class PlaygroundUIFactory : IPlaygroundUIFactory
    {
        private readonly IUIManager _uiManager;
        private readonly PlaygroundUIFactoryConfig _config;

        public PlaygroundUIFactory(IUIManager uiManager, PlaygroundUIFactoryConfig config)
        {
            _uiManager = uiManager;
            _config = config;
        }
        
        public IToolWindow CreateToolWindow()
        {
            return _uiManager.Instantiate(_config.ToolWindowPrefab);
        }

        public IToolButton CreateToolButton()
        {
            return _uiManager.Instantiate(_config.ToolButtonPrefab);
        }
    }
}