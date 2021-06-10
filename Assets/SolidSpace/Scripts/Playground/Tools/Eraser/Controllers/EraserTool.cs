using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.UI;
using SolidSpace.UI;

namespace SolidSpace.Playground.Tools.Eraser
{
    internal class EraserTool : IPlaygroundTool
    {
        public PlaygroundToolConfig Config { get; private set; }
        
        private readonly IEntityWorldManager _entityManager;
        private readonly IEntityByPositionSearchSystem _searchSystem;
        private readonly IPointerTracker _pointer;
        private readonly IUIManager _uiManager;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly IGizmosManager _gizmosManager;
        private readonly EraserToolConfig _config;

        private GizmosHandle _gizmos;
        private EraserToolWindow _window;

        public EraserTool(EraserToolConfig config, IEntityWorldManager entityManager, IGizmosManager gizmosManager,
            IEntityByPositionSearchSystem searchSystem, IPointerTracker pointer, IUIManager uiManager, IPlaygroundUIFactory uiFactory)
        {
            _entityManager = entityManager;
            _searchSystem = searchSystem;
            _pointer = pointer;
            _uiManager = uiManager;
            _uiFactory = uiFactory;
            _gizmosManager = gizmosManager;
            _config = config;
        }
        
        public void InitializeTool()
        {
            Config = _config.ToolConfig;

            _window = new EraserToolWindow(_uiFactory, _uiManager);
            _window.SetVisible(false);
            _gizmos = _gizmosManager.GetHandle(this);
        }
        
        public void OnToolActivation()
        {
            _searchSystem.SetEnabled(true);
            _window.SetVisible(true);
        }

        public void Update()
        {
            if (_uiManager.IsMouseOver)
            {
                return;
            }
            
            _searchSystem.SetSearchPosition(_pointer.Position);

            var result = _searchSystem.Result;
            if (!result.isValid)
            {
                return;
            }
            
            _gizmos.DrawLine(_pointer.Position, result.position, _config.GizmosColor);

            if (_pointer.ClickedThisFrame)
            {
                _entityManager.DestroyEntity(result.entity);
            }
        }

        public void OnToolDeactivation()
        {
            _searchSystem.SetEnabled(false);
            _window.SetVisible(false);
        }

        public void FinalizeTool()
        {
            
        }
    }
}