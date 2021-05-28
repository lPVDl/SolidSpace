using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.World
{
    internal class EntityWorldManager : IEntityWorldManager
    {
        private readonly Unity.Entities.World _world;

        public EntityWorldManager()
        {
            _world = new Unity.Entities.World("SolidSpace");
        }
        
        public void DestroyEntity(NativeSlice<Entity> entities)
        {
            _world.EntityManager.DestroyEntity(entities);
        }

        public void DestroyEntity(NativeArray<Entity> entities)
        {
            _world.EntityManager.DestroyEntity(entities);
        }

        public void DestroyEntity(Entity entity)
        {
            _world.EntityManager.DestroyEntity(entity);
        }

        public EntityQuery CreateEntityQuery(params ComponentType[] requiredComponents)
        {
            return _world.EntityManager.CreateEntityQuery(requiredComponents);
        }

        public ComponentTypeHandle<T> GetComponentTypeHandle<T>(bool isReadOnly)
        {
            return _world.EntityManager.GetComponentTypeHandle<T>(isReadOnly);
        }

        public EntityTypeHandle GetEntityTypeHandle()
        {
            return _world.EntityManager.GetEntityTypeHandle();
        }

        public EntityArchetype CreateArchetype(params ComponentType[] types)
        {
            return _world.EntityManager.CreateArchetype(types);
        }

        public NativeArray<Entity> CreateEntity(EntityArchetype archetype, int entityCount, Allocator allocator)
        {
            return _world.EntityManager.CreateEntity(archetype, entityCount, allocator);
        }

        public Entity CreateEntity(params ComponentType[] types)
        {
            return _world.EntityManager.CreateEntity(types);
        }

        public void SetComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData
        {
            _world.EntityManager.SetComponentData(entity, componentData);
        }

        public T GetComponentData<T>(Entity entity) where T : struct, IComponentData
        {
            return _world.EntityManager.GetComponentData<T>(entity);
        }
    }
}