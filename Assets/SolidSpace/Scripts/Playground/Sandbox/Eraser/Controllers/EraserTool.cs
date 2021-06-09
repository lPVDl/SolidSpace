using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Sandbox.Core;
using SolidSpace.Playground.UI;

namespace SolidSpace.Playground.Sandbox.Eraser
{
    internal class EraserTool : IPlaygroundTool
    {
        public PlaygroundToolConfig Config { get; private set; }
        
        private readonly IEntityWorldManager _entityManager;
        private readonly IEntityByPositionSearchSystem _searchSystem;
        private readonly IPointerTracker _pointer;
        private readonly IUIManager _uiManager;
        private readonly IGizmosManager _gizmosManager;
        private readonly EraserToolConfig _config;

        private GizmosHandle _gizmos;

        public EraserTool(EraserToolConfig config, IEntityWorldManager entityManager, IGizmosManager gizmosManager,
            IEntityByPositionSearchSystem searchSystem, IPointerTracker pointer, IUIManager uiManager)
        {
            _entityManager = entityManager;
            _searchSystem = searchSystem;
            _pointer = pointer;
            _uiManager = uiManager;
            _gizmosManager = gizmosManager;
            _config = config;
        }
        
        public void InitializeTool()
        {
            _gizmos = _gizmosManager.GetHandle(this);
            Config = _config.ToolConfig;
        }
        
        public void OnToolActivation()
        {
            _searchSystem.SetEnabled(true);
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
        }

        public void FinalizeTool()
        {
            
        }
    }
}