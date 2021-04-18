using Unity.Collections;
using Unity.Entities;

namespace SpaceSimulator.Entities.EntityWorld
{
    public interface IEntityManager
    {
        void DestroyEntity(NativeSlice<Entity> entities);
        EntityQuery CreateEntityQuery(params ComponentType[] requiredComponents);
        ComponentTypeHandle<T> GetComponentTypeHandle<T>(bool isReadOnly);
        EntityTypeHandle GetEntityTypeHandle();
        EntityArchetype CreateArchetype(params ComponentType[] types);
        NativeArray<Entity> CreateEntity(EntityArchetype archetype, int entityCount, Allocator allocator);
        void SetComponentData<T>(Entity entity, T componentData) where T : struct, IComponentData;
        Entity CreateEntity(params ComponentType[] types);
        void DestroyEntity(Entity entity);
    }
}