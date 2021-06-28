using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.World;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace SolidSpace.Entities.Bullets
{
    public struct BulletColliderWorld : IColliderWorld
    {
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<AtlasIndex> outHealthIndices;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<AtlasIndex> outSpriteIndices;
        
        [ReadOnly] private ComponentTypeHandle<SpriteRenderComponent> _spriteHandle;
        [ReadOnly] private ComponentTypeHandle<HealthComponent> _healthHandle;
        
        [NativeDisableContainerSafetyRestriction] private NativeArray<SpriteRenderComponent> _chunkSprites;
        [NativeDisableContainerSafetyRestriction] private NativeArray<HealthComponent> _chunkHealth;

        public void Initialize(IEntityManager entityManager, int colliderCount)
        {
            _spriteHandle = entityManager.GetComponentTypeHandle<SpriteRenderComponent>(true);
            _healthHandle = entityManager.GetComponentTypeHandle<HealthComponent>(true);
            outHealthIndices = NativeMemory.CreateTempJobArray<AtlasIndex>(colliderCount);
            outSpriteIndices = NativeMemory.CreateTempJobArray<AtlasIndex>(colliderCount);
        }

        public void ReadChunk(ArchetypeChunk chunk)
        {
            _chunkSprites = chunk.GetNativeArray(_spriteHandle);
            _chunkHealth = chunk.GetNativeArray(_healthHandle);
        }

        public void ReadEntity(int entityIndex, int writeOffset)
        {
            outHealthIndices[writeOffset] = _chunkHealth[entityIndex].index;
            outSpriteIndices[writeOffset] = _chunkSprites[entityIndex].index;
        }
        
        public void Dispose()
        {
            outHealthIndices.Dispose();
            outSpriteIndices.Dispose();
        }
    }
}