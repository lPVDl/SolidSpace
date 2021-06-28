using System;
using System.Runtime.CompilerServices;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Velcast;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Bullets
{
    internal struct BulletRaycastBehaviour : IRaycastBehaviour, IDisposable
    {
        [ReadOnly] public BakedCollidersData inColliders;
        [ReadOnly] public float inDeltaTime;

        [ReadOnly] public NativeArray<HealthComponent> inColliderHealths;
        [ReadOnly] public NativeSlice<AtlasChunk1D> inHealthChunks;
        [ReadOnly] public NativeArray<byte> inHealthAtlas;

        [ReadOnly] public NativeArray<SpriteRenderComponent> inColliderSprites;
        [ReadOnly] public NativeSlice<AtlasChunk2D> inSpriteChunks;
        
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<VelocityComponent> velocityHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;

        [NativeDisableContainerSafetyRestriction] private NativeArray<PositionComponent> _chunkPositions;
        [NativeDisableContainerSafetyRestriction] private NativeArray<VelocityComponent> _chunkVelocities;
        [NativeDisableContainerSafetyRestriction] private NativeArray<Entity> _chunkEntities;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<BulletHit> outHits;
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeReference<int> outCount;
    
        public void Initialize(int maxHitCount)
        {
            outHits = NativeMemory.CreateTempJobArray<BulletHit>(maxHitCount);
            outCount = NativeMemory.CreateTempJobReference<int>();
        }

        public void ReadChunk(ArchetypeChunk chunk)
        {
            _chunkPositions = chunk.GetNativeArray(positionHandle);
            _chunkVelocities = chunk.GetNativeArray(velocityHandle);
            _chunkEntities = chunk.GetNativeArray(entityHandle);
        }

        public FloatRay GetRay(int rayIndex)
        {
            var pos0 = _chunkPositions[rayIndex].value;
            var pos1 = pos0 + _chunkVelocities[rayIndex].value * inDeltaTime;
            
            return new FloatRay
            {
                pos0 = pos0,
                pos1 = pos1
            };
        }

        public bool TryRegisterHit(KovacRayHit hit)
        {
            var colliderShape = inColliders.shapes[hit.colliderIndex];
            var colliderBounds = inColliders.bounds[hit.colliderIndex];
            var colliderCenter = FloatMath.GetBoundsCenter(colliderBounds);
            var p0 = hit.ray.pos0 - colliderCenter;
            var p1 = hit.ray.pos1 - colliderCenter;
            FloatMath.SinCos(-colliderShape.rotation, out var sin, out var cos);
            var halfSize = new float2(colliderShape.size.x / 2f, colliderShape.size.y / 2f);
            p0 = FloatMath.Rotate(p0, sin, cos) + halfSize;
            p1 = FloatMath.Rotate(p1, sin, cos) + halfSize;
            
            var spriteSize = new int2((int) colliderShape.size.x, (int) colliderShape.size.y);
            var healthIndex = inColliderHealths[hit.colliderIndex].index;
            var healthOffset = AtlasMath.ComputeOffset(inHealthChunks[healthIndex.chunkId], healthIndex);
            
            var p0Int = new int2((int) p0.x, (int) p0.y);
            var p1Int = new int2((int) p1.x, (int) p1.y);
            if (p0Int.x == p1Int.x && p0Int.y == p1Int.y)
            {
                if (!CheckIndexBounds(p0Int.x, p0Int.y, spriteSize))
                {
                    return false;
                }
                    
                var offset = healthOffset + p0Int.y * spriteSize.x + p0Int.x;
                if (inHealthAtlas[offset] == 0)
                {
                    return false;
                }
                
                var spriteIndex = inColliderSprites[hit.colliderIndex].index;
                var spriteOffset = AtlasMath.ComputeOffset(inSpriteChunks[spriteIndex.chunkId], spriteIndex);
                spriteOffset += p0Int;
                    
                outHits[hit.writeOffset] = new BulletHit
                {
                    bulletEntity = _chunkEntities[hit.rayIndex],
                    spriteOffset = new ushort2(spriteOffset.x, spriteOffset.y),
                    healthOffset = offset
                };
                    
                return true;
            }
            
            var segmentCount = (int) Math.Ceiling(FloatMath.Distance(p0, p1)) + 1;
            var motion = (p1 - p0) / segmentCount;
            for (var j = 0; j <= segmentCount; j++)
            {
                var point = (int2) (p0 + motion * j);
                if (!CheckIndexBounds(point.x, point.y, spriteSize))
                {
                    continue;
                }

                var offset = healthOffset + point.y * spriteSize.x + point.x;
                if (inHealthAtlas[offset] == 0)
                {
                    continue;
                }

                var spriteIndex = inColliderSprites[hit.colliderIndex].index;
                var spriteOffset = AtlasMath.ComputeOffset(inSpriteChunks[spriteIndex.chunkId], spriteIndex);
                spriteOffset += point;
                    
                outHits[hit.writeOffset] = new BulletHit
                {
                    bulletEntity = _chunkEntities[hit.rayIndex],
                    spriteOffset = new ushort2(spriteOffset.x, spriteOffset.y),
                    healthOffset = offset
                };
                    
                return true;
            }

            return false;
        }

        public void CollectResult(NativeArray<int> offsets, NativeArray<int> counts)
        {
            new DataCollectJobWithOffsets<BulletHit>
            {
                inOffsets = offsets,
                inCounts = counts,
                inOutData = outHits,
                outCount = outCount
            }.Schedule().Complete();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckIndexBounds(int x, int y, int2 spriteSize)
        {
            if (x < 0 || y < 0 || x >= spriteSize.x || y >= spriteSize.y)
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            outHits.Dispose();
            outCount.Dispose();
        }
    }
}