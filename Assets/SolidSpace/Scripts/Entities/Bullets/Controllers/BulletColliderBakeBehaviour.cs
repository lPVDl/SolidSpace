using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.JobUtilities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace SolidSpace.Entities.Bullets
{
    public struct BulletColliderBakeBehaviour : IColliderBakeBehaviour
    {
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<HealthComponent> outHealthComponents;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<SpriteRenderComponent> outSpriteComponents;
        
        [ReadOnly] public ComponentTypeHandle<SpriteRenderComponent> spriteHandle;
        [ReadOnly] public ComponentTypeHandle<HealthComponent> healthHandle;
        
        [NativeDisableContainerSafetyRestriction] private NativeArray<SpriteRenderComponent> _chunkSprites;
        [NativeDisableContainerSafetyRestriction] private NativeArray<HealthComponent> _chunkHealth;

        public void Initialize(int colliderCount)
        {
            outHealthComponents = NativeMemory.CreateTempJobArray<HealthComponent>(colliderCount);
            outSpriteComponents = NativeMemory.CreateTempJobArray<SpriteRenderComponent>(colliderCount);
        }

        public void ReadChunk(ArchetypeChunk chunk)
        {
            _chunkSprites = chunk.GetNativeArray(spriteHandle);
            _chunkHealth = chunk.GetNativeArray(healthHandle);
        }

        public void ReadEntity(int entityIndex, int writeOffset)
        {
            outHealthComponents[writeOffset] = _chunkHealth[entityIndex];
            outSpriteComponents[writeOffset] = _chunkSprites[entityIndex];
        }
        
        public void Dispose()
        {
            outHealthComponents.Dispose();
            outSpriteComponents.Dispose();
        }
    }
}