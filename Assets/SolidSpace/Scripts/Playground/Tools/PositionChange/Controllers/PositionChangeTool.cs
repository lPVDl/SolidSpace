using System.Linq;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.ComponentFilter;
using SolidSpace.Playground.Tools.EntitySearch;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.PositionChange
{
    internal class PositionChangeTool : IPlaygroundTool
    {
        public PlaygroundToolConfig Config { get; }
        
        private readonly IPlaygroundUIManager _playgroundUIManager;
        private readonly IComponentFilterFactory _filterFactory;
        private readonly IUIManager _uiManager;
        private readonly IEntitySearchSystem _searchSystem;
        private readonly IGizmosManager _gizmosManager;
        private readonly IEntityWorldManager _entityManager;
        private readonly IPointerTracker _pointer;

        private IComponentFilter _filter;
        private GizmosHandle _gizmos;
        private bool _captured;
        private float2 _captureOffset;
        private Entity _capturedEntity;

        public PositionChangeTool(PlaygroundToolConfig config, IPlaygroundUIManager playgroundUIManager, IPointerTracker pointer,
            IComponentFilterFactory filterFactory, IUIManager uiManager, IEntitySearchSystem searchSystem, 
            IGizmosManager gizmosManager, IEntityWorldManager entityManager)
        {
            _playgroundUIManager = playgroundUIManager;
            _filterFactory = filterFactory;
            _uiManager = uiManager;
            _searchSystem = searchSystem;
            _gizmosManager = gizmosManager;
            _entityManager = entityManager;
            _pointer = pointer;
            Config = config;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this);
            _filter = _filterFactory.Create(typeof(PositionComponent));
            _filter.FilterModified += UpdateSearchSystemQuery;
        }

        public void OnUpdate()
        {
            if (_captured)
            {
                if (_pointer.IsHeldThisFrame && _entityManager.CheckExists(_capturedEntity))
                {
                    DrawRay(_pointer.Position, _pointer.Position + _captureOffset);
                    _entityManager.SetComponentData(_capturedEntity, new PositionComponent
                    {
                        value = _pointer.Position + _captureOffset
                    });
                    
                    return;
                }

                _captured = false;
            }
            
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
            
            DrawRay(_pointer.Position, result.nearestPosition);

            if (_pointer.ClickedThisFrame)
            {
                _captured = true;
                _capturedEntity = result.nearestEntity;
                _captureOffset = _entityManager.GetComponentData<PositionComponent>(_capturedEntity).value - _pointer.Position;
            }
        }

        private void DrawRay(float2 start, float2 end)
        {
            _gizmos.DrawLine(start, end, Color.yellow);
            _gizmos.DrawScreenSquare(end, 6, Color.yellow);
            _gizmos.DrawScreenSquare(end, 4, Color.blue);
        }

        public void OnActivate(bool isActive)
        {
            if (isActive)
            {
                UpdateSearchSystemQuery();
            }
            
            _playgroundUIManager.SetElementVisible(_filter, isActive);
            _searchSystem.SetEnabled(isActive);
        }
        
        private void UpdateSearchSystemQuery()
        {
            _searchSystem.SetQuery(new EntityQueryDesc
            {
                All = _filter.GetTagsWithState(ETagLabelState.Positive).ToArray(),
                None = _filter.GetTagsWithState(ETagLabelState.Negative).ToArray()
            });
        }

        public void OnFinalize()
        {
            
        }
    }
}