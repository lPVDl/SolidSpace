using System.Linq;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.ComponentFilter;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Entities;

namespace SolidSpace.Playground.Tools.Eraser
{
    internal class EraserTool : IPlaygroundTool
    {
        public PlaygroundToolConfig Config { get; private set; }
        
        private readonly IEntityWorldManager _entityManager;
        private readonly IEntityByPositionSearchSystem _searchSystem;
        private readonly IPointerTracker _pointer;
        private readonly IUIManager _uiManager;
        private readonly IComponentFilterFactory _filterFactory;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly IGizmosManager _gizmosManager;
        private readonly EraserToolConfig _config;
        
        private GizmosHandle _gizmos;
        private IToolWindow _window;
        private IComponentFilter _filter;

        public EraserTool(EraserToolConfig config, IEntityWorldManager entityManager, IGizmosManager gizmosManager,
            IEntityByPositionSearchSystem searchSystem, IPointerTracker pointer, IUIManager uiManager, 
            IComponentFilterFactory filterFactory, IPlaygroundUIFactory uiFactory)
        {
            _entityManager = entityManager;
            _searchSystem = searchSystem;
            _pointer = pointer;
            _uiManager = uiManager;
            _filterFactory = filterFactory;
            _uiFactory = uiFactory;
            _gizmosManager = gizmosManager;
            _config = config;
        }
        
        public void InitializeTool()
        {
            Config = _config.ToolConfig;
            _gizmos = _gizmosManager.GetHandle(this);

            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Component Filter");

            _filter = _filterFactory.Create();
            _filter.SetState(typeof(PositionComponent), new FilterState
            {
                isLocked = true,
                state = ETagLabelState.Positive
            });
            _filter.FilterModified += UpdateSearchSystemQuery;
            _window.AttachChild(_filter);
        }
        
        public void OnToolActivation()
        {
            UpdateSearchSystemQuery();
            _searchSystem.SetEnabled(true);
            _uiManager.AddToRoot(_window, "ContainerA");
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

        private void UpdateSearchSystemQuery()
        {
            _searchSystem.SetQuery(new EntityQueryDesc
            {
                All = _filter.GetTagsWithState(ETagLabelState.Positive).ToArray(),
                None = _filter.GetTagsWithState(ETagLabelState.Negative).ToArray()
            });
        }

        public void OnToolDeactivation()
        {
            _searchSystem.SetEnabled(false);
            _uiManager.RemoveFromRoot(_window, "ContainerA");
        }

        public void FinalizeTool()
        {
            
        }
    }
}