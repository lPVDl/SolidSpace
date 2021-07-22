using System.Collections.Generic;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Splitting
{
    public class SplittingCommandSystem : ISplittingCommandSystem, IInitializable, IUpdatable
    {
        private readonly IEntityManager _entityManager;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly ISpriteColorSystem _spriteSystem;

        private HashSet<Entity> _checkingQueue;
        private SplittingController _splittingController;
        private List<JobMemoryAllocator> _jobMemoryPool;
        private List<SplittingContext> _splittingContext;

        public SplittingCommandSystem(IEntityManager entityManager, IHealthAtlasSystem healthSystem, 
            ISpriteColorSystem spriteSystem)
        {
            _entityManager = entityManager;
            _healthSystem = healthSystem;
            _spriteSystem = spriteSystem;
        }
        
        public void OnInitialize()
        {
            _splittingController = new SplittingController(_entityManager, _healthSystem, _spriteSystem);
            _checkingQueue = new HashSet<Entity>();
            _jobMemoryPool = new List<JobMemoryAllocator>();
            _splittingContext = new List<SplittingContext>();
        }
        
        public void ScheduleSplittingCheck(Entity entity)
        {
            _checkingQueue.Add(entity);
        }

        public void OnUpdate()
        {
            foreach (var entity in _checkingQueue)
            {
                _splittingContext.Add(new SplittingContext
                {
                    state = ESplittingState.NotStarted,
                    entity = entity,
                    jobMemory = GetItemFromPoolOrCreate(_jobMemoryPool)
                });
            }
            _checkingQueue.Clear();

            while (true)
            {
                for (var i = _splittingContext.Count - 1; i >= 0; i--)
                {
                    var context = _splittingController.UpdateState(_splittingContext[i]);
                    if (context.state == ESplittingState.Completed)
                    {
                        context.jobMemory.DisposeAllocations();
                        _jobMemoryPool.Add(context.jobMemory);
                        _splittingContext.RemoveAt(i);
                        
                        continue;
                    }

                    _splittingContext[i] = context;
                }

                if (_splittingContext.Count == 0)
                {
                    break;
                }

                JobHandle simplestJob = default;
                var minDifficulty = int.MaxValue;
                for (var i = 0; i < _splittingContext.Count; i++)
                {
                    var context = _splittingContext[i];
                    if (context.jobDifficulty <= minDifficulty)
                    {
                        minDifficulty = context.jobDifficulty;
                        simplestJob = context.jobHandle;
                    }
                }
                
                simplestJob.Complete();
            }
            
            _spriteSystem.Texture.Apply();
        }
        
        private static T GetItemFromPoolOrCreate<T>(IList<T> pool) where T : new()
        {
            var itemCount = pool.Count;
            if (itemCount == 0)
            {
                return new T();
            }

            var item = pool[itemCount - 1];
            pool.RemoveAt(itemCount - 1);
            
            return item;
        }

        public void OnFinalize()
        {
            
        }
    }
}