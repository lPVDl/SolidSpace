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

        private List<NativeSlice<Entity>> _entitiesToDespawn;

        public EntityDestructionBuffer(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }
        
        public void OnInitialize()
        {
            _entitiesToDespawn = new List<NativeSlice<Entity>>();
        }

        public void OnUpdate()
        {
            var count = _entitiesToDespawn.Count;
            
            for (var i = 0; i < count; i++)
            {
                _entityManager.DestroyEntity(_entitiesToDespawn[i]);
            }
            
            _entitiesToDespawn.Clear();
        }

        public void ScheduleDestroy(NativeSlice<Entity> entities)
        {
            _entitiesToDespawn.Add(entities);
        }

        public void OnFinalize()
        {
            
        }
    }
}