using System.Collections.Generic;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Despawn
{
    internal class EntityDestructionBuffer : IEntityDestructionBuffer, IInitializable, IUpdatable
    {
        private readonly IEntityManager _entityManager;
        
        private HashSet<Entity> _despawnSingles;
        private List<NativeSlice<Entity>> _despawnSlices;

        public EntityDestructionBuffer(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }
        
        public void OnInitialize()
        {
            _despawnSlices = new List<NativeSlice<Entity>>();
            _despawnSingles = new HashSet<Entity>();
        }

        public void ScheduleDestroy(NativeSlice<Entity> entities)
        {
            _despawnSlices.Add(entities);
        }

        public void ScheduleDestroy(Entity entity)
        {
            _despawnSingles.Add(entity);
        }

        public void OnUpdate()
        {
            foreach (var slice in _despawnSlices)
            {
                _entityManager.DestroyEntity(slice);
            }

            foreach (var entity in _despawnSingles)
            {
                _entityManager.DestroyEntity(entity);
            }

            _despawnSlices.Clear();
            _despawnSingles.Clear();
        }
        
        public void OnFinalize()
        {
            
        }
    }
}