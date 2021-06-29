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

        public void OnInitialize(int colliderCount)
        {
            outHealthComponents = NativeMemory.CreateTempJobArray<HealthComponent>(colliderCount);
            outSpriteComponents = NativeMemory.CreateTempJobArray<SpriteRenderComponent>(colliderCount);
        }

        public void OnProcessChunk(ArchetypeChunk chunk)
        {
            _chunkSprites = chunk.GetNativeArray(spriteHandle);
            _chunkHealth = chunk.GetNativeArray(healthHandle);
        }

        public void OnProcessChunkEntity(int chunkEntityIndex, int colliderIndex)
        {
            outHealthComponents[colliderIndex] = _chunkHealth[chunkEntityIndex];
            outSpriteComponents[colliderIndex] = _chunkSprites[chunkEntityIndex];
        }
        
        public void Dispose()
        {
            outHealthComponents.Dispose();
            outSpriteComponents.Dispose();
        }
    }
}