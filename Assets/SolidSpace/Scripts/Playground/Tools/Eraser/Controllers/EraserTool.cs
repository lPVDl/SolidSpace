using System.Collections.Generic;
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
        private readonly IComponentFilterTool _filterTool;
        private readonly IComponentFilterMaster _filterMaster;
        private readonly IGizmosManager _gizmosManager;
        private readonly EraserToolConfig _config;

        private FilterState[] _filter;
        private GizmosHandle _gizmos;
        private List<ComponentType> _whitelist;
        private List<ComponentType> _blacklist;

        public EraserTool(EraserToolConfig config, IEntityWorldManager entityManager, IGizmosManager gizmosManager,
            IEntityByPositionSearchSystem searchSystem, IPointerTracker pointer, IUIManager uiManager, 
            IComponentFilterTool filterTool, IComponentFilterMaster filterMaster)
        {
            _entityManager = entityManager;
            _searchSystem = searchSystem;
            _pointer = pointer;
            _uiManager = uiManager;
            _filterTool = filterTool;
            _filterMaster = filterMaster;
            _gizmosManager = gizmosManager;
            _config = config;
        }
        
        public void InitializeTool()
        {
            Config = _config.ToolConfig;
            _gizmos = _gizmosManager.GetHandle(this);
            _filter = _filterMaster.CreateFilter();
            _filterMaster.ModifyFilter(_filter, typeof(PositionComponent), new FilterState
            {
                isLocked = true,
                state = ETagLabelState.Positive
            });
            _whitelist = new List<ComponentType>(_filterMaster.AllComponents.Count);
            _blacklist = new List<ComponentType>(_filterMaster.AllComponents.Count);
        }
        
        public void OnToolActivation()
        {
            _searchSystem.SetQuery(BuildQuery(_filter));
            _searchSystem.SetEnabled(true);
            _filterTool.SetFilter(_filter);
            _filterTool.SetEnabled(true);
            _filterTool.FilterModified += OnFilterModified;
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

        private void OnFilterModified()
        {
            _filterTool.GetFilter(_filter);
            var query = BuildQuery(_filter);
            _searchSystem.SetQuery(query);
        }

        public void OnToolDeactivation()
        {
            _searchSystem.SetEnabled(false);
            _filterTool.GetFilter(_filter);
            _filterTool.SetEnabled(false);
            _filterTool.FilterModified -= OnFilterModified;
        }
        
        private EntityQueryDesc BuildQuery(FilterState[] filter)
        {
            _filterMaster.SplitFilter(filter, _whitelist, _blacklist);
            
            return new EntityQueryDesc
            {
                All = _whitelist.ToArray(),
                None = _blacklist.ToArray()
            };
        }

        public void FinalizeTool()
        {
            
        }
    }
}