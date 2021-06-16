using SolidSpace.Playground.Core;
using SolidSpace.Playground.UI;
using SolidSpace.UI;

namespace SolidSpace.Playground.Tools.SpawnPoint
{
    internal class SpawnPointToolFactory : ISpawnPointToolFactory
    {
        private readonly IUIManager _uiManager;
        private readonly IPointerTracker _pointer;
        private readonly IPlaygroundUIFactory _uiFactory;

        public SpawnPointToolFactory(IUIManager uiManager, IPointerTracker pointer, IPlaygroundUIFactory uiFactory)
        {
            _uiManager = uiManager;
            _pointer = pointer;
            _uiFactory = uiFactory;
        }
        
        public ISpawnPointTool Create()
        {
            var window = _uiFactory.CreateToolWindow();
            window.SetTitle("Spawn Config");

            var tool = new SpawnPointTool
            {
                UIManager = _uiManager,
                Pointer = _pointer,
                Window = window
            };
            
            return tool;
        }
    }
}