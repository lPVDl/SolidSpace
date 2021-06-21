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

namespace SolidSpace.Playground.Tools.VelocityChange
{
    internal class VelocityChangeTool : IPlaygroundTool
    {
        public PlaygroundToolConfig Config { get; }
        
        private readonly IGizmosManager _gizmosManager;
        private readonly IUIManager _uiManager;
        private readonly IComponentFilterFactory _filterFactory;
        private readonly IEntitySearchSystem _searchSystem;
        private readonly IPlaygroundUIManager _playgroundUIManager;
        private readonly IPointerTracker _pointer;
        private readonly IEntityWorldManager _entityManager;

        private IComponentFilter _filter;
        private GizmosHandle _gizmos;
        private bool _captured;
        private float2 _pointerClickPosition;
        private float2 _entityClickPosition;
        private Entity _capturedEntity;

        public VelocityChangeTool(PlaygroundToolConfig config, IGizmosManager gizmosManager, IUIManager uiManager,
            IComponentFilterFactory filterFactory, IEntitySearchSystem searchSystem, IPlaygroundUIManager playgroundUIManager,
            IPointerTracker pointer, IEntityWorldManager entityManager)
        {
            _gizmosManager = gizmosManager;
            _uiManager = uiManager;
            _filterFactory = filterFactory;
            _searchSystem = searchSystem;
            _playgroundUIManager = playgroundUIManager;
            _pointer = pointer;
            _entityManager = entityManager;
            Config = config;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this);
            _filter = _filterFactory.Create(typeof(PositionComponent), typeof(VelocityComponent));
            _filter.FilterModified += UpdateSearchSystemQuery;
        }

        public void OnUpdate()
        {
            if (_captured)
            {
                var entityExists = _entityManager.CheckExists(_capturedEntity);
                var pointerDelta = _pointer.Position - _pointerClickPosition;
                
                if (_pointer.IsHeldThisFrame && entityExists)
                {
                    DrawRay(_entityClickPosition + pointerDelta, _entityClickPosition);
                    
                    _entityManager.SetComponentData(_capturedEntity, new PositionComponent
                    {
                        value = _entityClickPosition
                    });
                    _entityManager.SetComponentData(_capturedEntity, new VelocityComponent()
                    {
                        value = float2.zero
                    });
                    
                    return;
                }
                
                if (entityExists)
                {
                    _entityManager.SetComponentData(_capturedEntity, new VelocityComponent()
                    {
                        value = pointerDelta
                    });
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
                _pointerClickPosition = _pointer.Position;
                _entityClickPosition = result.nearestPosition;
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