using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.SpawnPoint;
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

        private EntityArchetype _emitterArchetype;
        private ISpawnPointTool _spawnPointTool;

        public EmitterSpawnTool(PlaygroundToolConfig config, IEntityWorldManager entityManager,
            ISpawnPointToolFactory spawnPointToolFactory)
        {
            _entityManager = entityManager;
            _spawnPointToolFactory = spawnPointToolFactory;
            Config = config;
        }
        
        public void InitializeTool()
        {
            _spawnPointTool = _spawnPointToolFactory.Create();
            _emitterArchetype = _entityManager.CreateArchetype(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(ParticleEmitterComponent),
                typeof(RandomComponent),
                typeof(RepeatTimerComponent),
            });
        }

        public void Update()
        {
            foreach (var point in _spawnPointTool.Update())
            {
                Spawn(point);
            }
        }

        private void Spawn(float2 position)
        {
            var entity = _entityManager.CreateEntity(_emitterArchetype);
            _entityManager.SetComponentData(entity, new PositionComponent
            {
                value = position
            });
            _entityManager.SetComponentData(entity, new RepeatTimerComponent
            {
                delay = 1 / 60f
            });
            _entityManager.SetComponentData(entity, new ParticleEmitterComponent
            {
                particleVelocity = 5
            });
        }

        public void OnToolActivation()
        {
            _spawnPointTool.SetEnabled(true);
        }

        public void OnToolDeactivation()
        {
            _spawnPointTool.SetEnabled(false);
        }

        public void FinalizeTool()
        {
            
        }
    }
}