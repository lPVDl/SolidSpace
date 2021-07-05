using SolidSpace.UI.Core;

namespace SolidSpace.UI.Factory
{
    internal class UIFactory : IUIFactory
    {
        private readonly IUIManager _uiManager;
        private readonly UIPrefabs _prefabs;

        public UIFactory(IUIManager uiManager, UIPrefabs prefabs)
        {
            _uiManager = uiManager;
            _prefabs = prefabs;
        }
        
        public IToolWindow CreateToolWindow() => _uiManager.Instantiate(_prefabs.ToolWindow);

        public IToolButton CreateToolButton() => _uiManager.Instantiate(_prefabs.ToolButton);

        public ITagLabel CreateTagLabel() => _uiManager.Instantiate(_prefabs.TagLabel);

        public ILayoutGrid CreateLayoutGrid() => _uiManager.Instantiate(_prefabs.LayoutGrid);

        public IGeneralButton CreateGeneralButton() => _uiManager.Instantiate(_prefabs.GeneralButton);

        public IStringField CreateStringField() => _uiManager.Instantiate(_prefabs.StringField);
        
        public IVerticalFixedItemList CreateVerticalList() => _uiManager.Instantiate(_prefabs.VerticalFixedItemList);
    }
}