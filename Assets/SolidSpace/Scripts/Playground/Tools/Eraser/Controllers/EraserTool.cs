using System.Linq;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Entities.SearchNearestEntity;
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
        private readonly ISearchNearestEntitySystem _searchSystem;
        private readonly IPointerTracker _pointer;
        private readonly IPlaygroundUIManager _playgroundUIManager;
        private readonly IComponentFilterFactory _filterFactory;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly IUIManager _uiManager;
        private readonly IGizmosManager _gizmosManager;
        private readonly EraserToolConfig _config;
        
        private GizmosHandle _gizmos;
        private IToolWindow _window;
        private IComponentFilter _filter;

        public EraserTool(EraserToolConfig config, IEntityWorldManager entityManager, IGizmosManager gizmosManager,
            ISearchNearestEntitySystem searchSystem, IPointerTracker pointer, IPlaygroundUIManager playgroundUIManager, 
            IComponentFilterFactory filterFactory, IPlaygroundUIFactory uiFactory, IUIManager uiManager)
        {
            _entityManager = entityManager;
            _searchSystem = searchSystem;
            _pointer = pointer;
            _playgroundUIManager = playgroundUIManager;
            _filterFactory = filterFactory;
            _uiFactory = uiFactory;
            _uiManager = uiManager;
            _gizmosManager = gizmosManager;
            _config = config;
        }
        
        public void OnInitialize()
        {
            Config = _config.ToolConfig;
            _gizmos = _gizmosManager.GetHandle(this);

            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Eraser");

            _filter = _filterFactory.Create(typeof(PositionComponent));
            _filter.FilterModified += UpdateSearchSystemQuery;

            var button = _uiFactory.CreateGeneralButton();
            button.SetLabel("Destroy matching");
            button.Clicked += OnDestroyClicked;
            _window.AttachChild(button);
        }
        
        public void OnActivate(bool isActive)
        {
            if (isActive)
            {
                UpdateSearchSystemQuery();
            }
            
            _searchSystem.SetEnabled(isActive);
            _playgroundUIManager.SetElementVisible(_filter, isActive);
            _playgroundUIManager.SetElementVisible(_window, isActive);
        }

        public void OnUpdate()
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

        private void OnDestroyClicked()
        {
            var query = _entityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = _filter.GetTagsWithState(ETagLabelState.Positive).ToArray(),
                None = _filter.GetTagsWithState(ETagLabelState.Negative).ToArray()
            });
            
            _entityManager.DestroyEntity(query);
        }

        public void OnFinalize()
        {
            
        }
    }
}