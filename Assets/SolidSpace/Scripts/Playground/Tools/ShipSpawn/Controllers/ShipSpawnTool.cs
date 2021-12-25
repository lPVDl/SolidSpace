using System;
using SolidSpace.Entities.Prefabs;
using SolidSpace.Gizmos;
using SolidSpace.Mathematics;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.ComponentFilter;
using SolidSpace.Playground.Tools.Spawn;
using SolidSpace.UI.Core;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ShipSpawn
{
    internal class ShipSpawnTool : IPlaygroundTool, ISpawnToolHandler
    {
        private readonly ISpawnToolFactory _spawnToolFactory;
        private readonly IPlaygroundUIManager _uiManager;
        private readonly IComponentFilterFactory _filterFactory;
        private readonly IGizmosManager _gizmosManager;
        private readonly IPrefabSystem _prefabSystem;

        private ISpawnTool _spawnTool;
        private IUIElement _componentsWindow;
        private GizmosHandle _gizmos;

        public ShipSpawnTool(ISpawnToolFactory spawnToolFactory,
                             IPlaygroundUIManager uiManager,
                             IComponentFilterFactory filterFactory,
                             IGizmosManager gizmosManager,
                             IPrefabSystem prefabSystem)
        {
            _spawnToolFactory = spawnToolFactory;
            _uiManager = uiManager;
            _filterFactory = filterFactory;
            _gizmosManager = gizmosManager;
            _prefabSystem = prefabSystem;
        }

        public void OnInitialize()
        {
            _componentsWindow = _filterFactory.CreateReadonly(_prefabSystem.ShipComponents);
            _spawnTool = _spawnToolFactory.Create(this);
            _gizmos = _gizmosManager.GetHandle(this, Color.yellow);
        }
        
        public void OnFinalize()
        {
            
        }
        
        public void OnActivate(bool isActive)
        {
            _spawnTool.OnActivate(isActive);       
            _uiManager.SetElementVisible(_componentsWindow, isActive);
        }
        
        public void OnUpdate()
        {
            _spawnTool.OnUpdate();
        }
        
        public void OnSpawnEvent(SpawnEventData eventData)
        {
            var origin = eventData.origin;
            
            switch (eventData.eventType)
            {
                case ESpawnEventType.Preview:
                    _gizmos.DrawWireRect(origin.position, _prefabSystem.ShipSize, origin.rotation);
                    var xAxis = FloatMath.Rotate(_prefabSystem.ShipSize.x * 0.5f, origin.rotation);
                    _gizmos.DrawLine(origin.position, origin.position + xAxis);
                    break;
                
                case ESpawnEventType.Place:
                    _prefabSystem.SpawnShip(origin.position, origin.rotation);

                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnDrawSpawnCircle(float2 position, float radius)
        {
            _gizmos.DrawScreenCircle(position, radius);
        }
    }
}