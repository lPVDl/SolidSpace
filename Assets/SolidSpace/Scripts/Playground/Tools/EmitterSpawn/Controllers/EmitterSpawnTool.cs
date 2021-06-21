using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.SpawnPoint;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.EmitterSpawn
{
    public class EmitterSpawnTool : IPlaygroundTool
    {
        public PlaygroundToolConfig Config { get; }

        private readonly IEntityWorldManager _entityManager;
        private readonly ISpawnPointToolFactory _spawnPointToolFactory;
        private readonly IUIManager _uiManager;
        private readonly IPlaygroundUIFactory _uiFactory;

        private EntityArchetype _emitterArchetype;
        private ISpawnPointTool _spawnPointTool;
        private IToolWindow _window;
        private IStringField _spawnRateField;
        private IStringField _particleVelocityField;

        private float _spawnRate;
        private float _particleVelocity;

        public EmitterSpawnTool(PlaygroundToolConfig config, IEntityWorldManager entityManager,
            ISpawnPointToolFactory spawnPointToolFactory, IUIManager uiManager, IPlaygroundUIFactory uiFactory)
        {
            _entityManager = entityManager;
            _spawnPointToolFactory = spawnPointToolFactory;
            _uiManager = uiManager;
            _uiFactory = uiFactory;
            Config = config;
        }
        
        public void OnInitialize()
        {
            _spawnPointTool = _spawnPointToolFactory.Create();
            _emitterArchetype = _entityManager.CreateArchetype(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ParticleEmitterComponent),
                typeof(RandomComponent),
                typeof(RepeatTimerComponent),
            });

            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Emitter Config");

            _spawnRate = 60;
            _spawnRateField = _uiFactory.CreateStringField();
            _spawnRateField.SetValue("60");
            _spawnRateField.SetLabel("Spawn Rate");
            _spawnRateField.SetValueCorrectionBehaviour(new FloatMinMaxBehaviour(1, 60));
            _spawnRateField.ValueChanged += () => _spawnRate = float.Parse(_spawnRateField.Value);
            _window.AttachChild(_spawnRateField);

            _particleVelocity = 10;
            _particleVelocityField = _uiFactory.CreateStringField();
            _particleVelocityField.SetValue("10");
            _particleVelocityField.SetLabel("Particle Velocity");
            _particleVelocityField.SetValueCorrectionBehaviour(new FloatMinMaxBehaviour(0, 1000));
            _particleVelocityField.ValueChanged += () => _particleVelocity = float.Parse(_particleVelocityField.Value);
            _window.AttachChild(_particleVelocityField);
        }

        public void OnUpdate()
        {
            foreach (var point in _spawnPointTool.OnUpdate())
            {
                Spawn(point, _spawnRate, _particleVelocity);
            }
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

        public void OnActivate(bool isActive)
        {
            if (isActive)
            {
                _spawnPointTool.OnActivate(true);
                _uiManager.AddToRoot(_window, "ContainerA");
            }
            else
            {
                _spawnPointTool.OnActivate(false);
                _uiManager.RemoveFromRoot(_window, "ContainerA");
            }
        }

        public void OnFinalize()
        {
            
        }
    }
}