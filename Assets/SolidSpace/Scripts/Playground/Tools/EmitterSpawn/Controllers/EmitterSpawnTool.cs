using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Playground.Core;
using SolidSpace.UI;
using Unity.Entities;

namespace SolidSpace.Playground.Tools.EmitterSpawn
{
    public class EmitterSpawnTool : IPlaygroundTool
    {
        public PlaygroundToolConfig Config { get; }

        private readonly IEntityWorldManager _entityManager;
        private readonly IUIManager _uiManager;
        private readonly IPointerTracker _pointer;

        private EntityArchetype _emitterArchetype;

        public EmitterSpawnTool(PlaygroundToolConfig config, IEntityWorldManager entityManager, IUIManager uiManager,
            IPointerTracker pointer)
        {
            _entityManager = entityManager;
            _uiManager = uiManager;
            _pointer = pointer;
            Config = config;
        }
        
        public void InitializeTool()
        {
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
            if (_uiManager.IsMouseOver || !_pointer.ClickedThisFrame)
            {
                return;
            }

            var entity = _entityManager.CreateEntity(_emitterArchetype);
            _entityManager.SetComponentData(entity, new PositionComponent
            {
                value = _pointer.Position
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
            
        }

        public void OnToolDeactivation()
        {
            
        }

        public void FinalizeTool()
        {
            
        }
    }
}