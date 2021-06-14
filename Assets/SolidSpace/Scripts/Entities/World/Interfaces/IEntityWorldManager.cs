using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.World
{
    public interface IEntityWorldManager
    {
        EntityQuery CreateEntityQuery(params ComponentType[] requiredComponents);
        ComponentTypeHandle<T> GetComponentTypeHandle<T>(bool isReadOnly);
        EntityTypeHandle GetEntityTypeHandle();
        EntityArchetype CreateArchetype(params ComponentType[] types);
        NativeArray<Entity> CreateEntity(EntityArchetype archetype, int entityCount, Allocator allocator);
        void SetComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData;
        T GetComponentData<T>(Entity entity) where T : struct, IComponentData;
        Entity CreateEntity(params ComponentType[] types);
        void DestroyEntity(Entity entity);
        void DestroyEntity(NativeSlice<Entity> entities);
        void DestroyEntity(NativeArray<Entity> entities);
        NativeArray<Entity> GetAllEntities(Allocator allocator);
        Entity CreateEntity(EntityArchetype archetype);
        EntityQuery CreateEntityQuery(params EntityQueryDesc[] queryDesc);
    }
}