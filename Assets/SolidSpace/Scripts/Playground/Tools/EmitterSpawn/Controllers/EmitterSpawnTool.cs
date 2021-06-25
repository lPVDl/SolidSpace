using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.ComponentFilter;
using SolidSpace.Playground.Tools.Spawn;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.EmitterSpawn
{
    public class EmitterSpawnTool : IPlaygroundTool, ISpawnToolHandler
    {
        private readonly IEntityManager _entityManager;
        private readonly ISpawnToolFactory _spawnToolFactory;
        private readonly IPlaygroundUIManager _uiManager;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly IComponentFilterFactory _filterFactory;
        private readonly IGizmosManager _gizmosManager;

        private EntityArchetype _emitterArchetype;
        private ISpawnTool _spawnTool;
        private IToolWindow _emitterWindow;
        private IUIElement _componentsWindow;
        private IStringField _spawnRateField;
        private IStringField _particleVelocityField;
        private GizmosHandle _gizmos;

        private float _spawnRate;
        private float _particleVelocity;

        public EmitterSpawnTool(IEntityManager entityManager, IPlaygroundUIManager uiManager,
            ISpawnToolFactory spawnToolFactory, IPlaygroundUIFactory uiFactory, IComponentFilterFactory filterFactory,
            IGizmosManager gizmosManager)
        {
            _entityManager = entityManager;
            _spawnToolFactory = spawnToolFactory;
            _uiManager = uiManager;
            _uiFactory = uiFactory;
            _filterFactory = filterFactory;
            _gizmosManager = gizmosManager;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this, Color.yellow);
            
            var emitterComponents = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ParticleEmitterComponent),
                typeof(RandomValueComponent),
                typeof(RepeatTimerComponent),
            };
            _emitterArchetype = _entityManager.CreateArchetype(emitterComponents);

            _componentsWindow = _filterFactory.CreateReadonly(emitterComponents);

            _emitterWindow = _uiFactory.CreateToolWindow();
            _emitterWindow.SetTitle("Emitter");

            _spawnTool = _spawnToolFactory.Create(this);

            _spawnRate = 60;
            _spawnRateField = _uiFactory.CreateStringField();
            _spawnRateField.SetValue("60");
            _spawnRateField.SetLabel("Spawn Rate");
            _spawnRateField.SetValueCorrectionBehaviour(new FloatMinMaxBehaviour(1, 60));
            _spawnRateField.ValueChanged += () => _spawnRate = float.Parse(_spawnRateField.Value);
            _emitterWindow.AttachChild(_spawnRateField);

            _particleVelocity = 10;
            _particleVelocityField = _uiFactory.CreateStringField();
            _particleVelocityField.SetValue("10");
            _particleVelocityField.SetLabel("Particle Velocity");
            _particleVelocityField.SetValueCorrectionBehaviour(new FloatMinMaxBehaviour(0, 1000));
            _particleVelocityField.ValueChanged += () => _particleVelocity = float.Parse(_particleVelocityField.Value);
            _emitterWindow.AttachChild(_particleVelocityField);
        }
        
        public void OnActivate(bool isActive)
        {
            _spawnTool.OnActivate(isActive);
            _uiManager.SetElementVisible(_componentsWindow, isActive);
            _uiManager.SetElementVisible(_emitterWindow, isActive);
        }

        public void OnUpdate()
        {
            _spawnTool.OnUpdate();
        }
        
        public void OnSpawnEvent(SpawnEventData eventData)
        {
            switch (eventData.eventType)
            {
                case ESpawnEventType.Preview:
                    _gizmos.DrawScreenSquare(eventData.position, 6);
                    break;
                
                case ESpawnEventType.Place:
                    Spawn(eventData.position, _spawnRate, _particleVelocity);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnDrawSpawnCircle(float2 position, float radius)
        {
            _gizmos.DrawWirePolygon(position, radius, 48);
        }

        private void Spawn(float2 position, float spawnRate, float particleVelocity)
        {
            var entity = _entityManager.CreateEntity(_emitterArchetype);
            _entityManager.SetComponentData(entity, new PositionComponent
            {
                value = position
            });
            _entityManager.SetComponentData(entity, new RepeatTimerComponent
            {
                delay = 1f / spawnRate
            });
            _entityManager.SetComponentData(entity, new ParticleEmitterComponent
            {
                particleVelocity = particleVelocity
            });
        }

        public void OnFinalize() { }
    }
}