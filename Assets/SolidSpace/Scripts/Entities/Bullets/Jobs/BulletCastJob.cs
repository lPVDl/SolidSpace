using System;
using System.Runtime.CompilerServices;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.Physics.Raycast;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Bullets
{
    [BurstCompile]
    internal struct BulletCastJob : IJobParallelFor
    {
        [ReadOnly] public int inItemPerJob;
        [ReadOnly] public int inItemTotal;
        [ReadOnly] public NativeArray<HealthComponent> inHealthComponents;
        [ReadOnly] public NativeArray<SpriteComponent> inSpriteComponents;
        [ReadOnly] public NativeArray<byte> inHealthAtlas;
        [ReadOnly] public NativeSlice<AtlasChunk1D> inHealthChunks;
        [ReadOnly] public NativeSlice<AtlasChunk2D> inSpriteChunks;
        [ReadOnly] public RaycastWorld inRaycastWorld;
        [ReadOnly] public ColliderWorld inColliderWorld;
        [ReadOnly] public NativeArray<int> inFilteredIndices;

        [WriteOnly] public NativeArray<int> outCounts;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<BulletHit> outHits;
        
        public void Execute(int jobIndex)
        {
            var startIndex = jobIndex * inItemPerJob;
            var endIndex = Math.Min(startIndex + inItemPerJob, inItemTotal);
            var hitCount = 0;
            
            for (var i = startIndex; i < endIndex; i++)
            {
                var filterIndex = inFilteredIndices[i];
                var colliderIndex = inRaycastWorld.colliderIndices[filterIndex];
                var colliderShape = inColliderWorld.colliderShapes[colliderIndex];
                var colliderBounds = inColliderWorld.colliderBounds[colliderIndex];
                var colliderCenter = FloatMath.GetBoundsCenter(colliderBounds);
                var ray = inRaycastWorld.raycastOrigins[filterIndex];
                var p0 = ray.pos0 - colliderCenter;
                var p1 = ray.pos1 - colliderCenter;
                FloatMath.SinCos(-colliderShape.rotation * FloatMath.TwoPI, out var sin, out var cos);
                var halfSize = new float2(colliderShape.size.x / 2f, colliderShape.size.y / 2f);
                p0 = FloatMath.Rotate(p0, sin, cos) + halfSize;
                p1 = FloatMath.Rotate(p1, sin, cos) + halfSize;
                
                var spriteSize = new int2((int) colliderShape.size.x, (int) colliderShape.size.y);
                var healthIndex = inHealthComponents[colliderIndex].index;
                var healthOffset = AtlasMath.ComputeOffset(inHealthChunks[healthIndex.chunkId], healthIndex);

                var p0Int = new int2((int) p0.x, (int) p0.y);
                var p1Int = new int2((int) p1.x, (int) p1.y);
                if (p0Int.x == p1Int.x && p0Int.y == p1Int.y)
                {
                    if (!CheckIndexBounds(p0Int.x, p0Int.y, spriteSize))
                    {
                        continue;
                    }
                    
                    var offset = healthOffset + p0Int.y * spriteSize.x + p0Int.x;
                    if (inHealthAtlas[offset] == 0)
                    {
                        continue;
                    }
                
                    var spriteIndex = inSpriteComponents[colliderIndex].index;
                    var spriteOffset = AtlasMath.ComputeOffset(inSpriteChunks[spriteIndex.chunkId], spriteIndex);
                    spriteOffset += p0Int;
                    
                    outHits[startIndex + hitCount++] = new BulletHit
                    {
                        bulletEntity = inRaycastWorld.raycastEntities[filterIndex],
                        spriteOffset = new ushort2(spriteOffset.x, spriteOffset.y),
                        healthOffset = offset
                    };
                    
                    continue;
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

                    var spriteIndex = inSpriteComponents[colliderIndex].index;
                    var spriteOffset = AtlasMath.ComputeOffset(inSpriteChunks[spriteIndex.chunkId], spriteIndex);
                    spriteOffset += point;
                    
                    outHits[startIndex + hitCount++] = new BulletHit
                    {
                        bulletEntity = inRaycastWorld.raycastEntities[filterIndex],
                        spriteOffset = new ushort2(spriteOffset.x, spriteOffset.y),
                        healthOffset = offset
                    };
                    
                    break;
                }
            }

            outCounts[jobIndex] = hitCount;
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
    }
}