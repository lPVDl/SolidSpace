using System.Threading;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.World
{
    internal class EntityManager : IEntityManager
    {
        private readonly Unity.Entities.World _world;

        public EntityManager()
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

        public void DestroyEntity(EntityQuery query)
        {
            _world.EntityManager.DestroyEntity(query);
        }

        public void DestroyEntity(Entity entity)
        {
            _world.EntityManager.DestroyEntity(entity);
        }

        public NativeArray<Entity> GetAllEntities(Allocator allocator)
        {
            return _world.EntityManager.GetAllEntities(allocator);
        }

        public EntityQuery CreateEntityQuery(params ComponentType[] requiredComponents)
        {
            return _world.EntityManager.CreateEntityQuery(requiredComponents);
        }

        public EntityQuery CreateEntityQuery(params EntityQueryDesc[] queryDesc)
        {
            return _world.EntityManager.CreateEntityQuery(queryDesc);
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

        public Entity CreateEntity(EntityArchetype archetype)
        {
            return _world.EntityManager.CreateEntity(archetype);
        }

        public void SetComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData
        {
            _world.EntityManager.SetComponentData(entity, componentData);
        }

        public bool CheckExists(Entity entity)
        {
            return _world.EntityManager.Exists(entity);
        }

        public T GetComponentData<T>(Entity entity) where T : struct, IComponentData
        {
            return _world.EntityManager.GetComponentData<T>(entity);
        }
    }
}