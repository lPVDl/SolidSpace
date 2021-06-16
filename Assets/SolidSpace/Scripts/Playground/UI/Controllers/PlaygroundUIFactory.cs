using SolidSpace.UI;

namespace SolidSpace.Playground.UI
{
    internal class PlaygroundUIFactory : IPlaygroundUIFactory
    {
        private readonly IUIManager _uiManager;
        private readonly PlaygroundUIPrefabs _prefabs;

        public PlaygroundUIFactory(IUIManager uiManager, PlaygroundUIPrefabs prefabs)
        {
            _uiManager = uiManager;
            _prefabs = prefabs;
        }
        
        public IToolWindow CreateToolWindow()
        {
            return _uiManager.Instantiate(_prefabs.ToolWindow);
        }

        public IToolButton CreateToolButton() => _uiManager.Instantiate(_prefabs.ToolButton);

        public ITagLabel CreateTagLabel() => _uiManager.Instantiate(_prefabs.TagLabel);

        public ILayoutGrid CreateLayoutGrid() => _uiManager.Instantiate(_prefabs.LayoutGrid);

        public IGeneralButton CreateGeneralButton() => _uiManager.Instantiate(_prefabs.GeneralButton);

        public IStringField CreateStringField() => _uiManager.Instantiate(_prefabs.StringField);
    }
}