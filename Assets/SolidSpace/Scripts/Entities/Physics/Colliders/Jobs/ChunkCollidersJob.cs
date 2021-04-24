using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics
{
    [BurstCompile]
    public struct ChunkCollidersJob : IJobParallelFor
    {
        [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<FloatBounds> inColliderBounds;
        [ReadOnly] public int inColliderTotalCount;
        [ReadOnly] public int inColliderPerJob;
        [ReadOnly] public ColliderWorldGrid inWorldGrid;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ChunkedCollider> outColliders;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<int> outColliderCount;
        
        public void Execute(int jobIndex)
        {
            var startIndex = jobIndex * inColliderPerJob;
            var endIndex = math.min(startIndex + inColliderPerJob, inColliderTotalCount);
            var colliderCount = 0;
            var writeOffset = jobIndex * inColliderPerJob * 4;
            var worldPower = inWorldGrid.power;
            var worldAnchor = inWorldGrid.anchor;
            var worldSize = inWorldGrid.size;

            for (var i = startIndex; i < endIndex; i++)
            {
                var bounds = inColliderBounds[i];
                var x0 = ((int) bounds.xMin >> worldPower) - worldAnchor.x;
                var y0 = ((int) bounds.yMin >> worldPower) - worldAnchor.y;
                var x1 = ((int) bounds.xMax >> worldPower) - worldAnchor.x;
                var y1 = ((int) bounds.yMax >> worldPower) - worldAnchor.y;
                var anchorChunk = y0 * worldSize.x + x0;

                ChunkedCollider chunkedCollider;
                chunkedCollider.colliderIndex = (ushort) i;
                
                chunkedCollider.chunkIndex = (ushort) anchorChunk;
                outColliders[writeOffset + colliderCount++] = chunkedCollider;

                if (x0 != x1)
                {
                    chunkedCollider.chunkIndex++;
                    outColliders[writeOffset + colliderCount++] = chunkedCollider;

                    if (y0 != y1)
                    {
                        chunkedCollider.chunkIndex = (ushort) (anchorChunk + worldSize.x);
                        outColliders[writeOffset + colliderCount++] = chunkedCollider;

                        chunkedCollider.chunkIndex++;
                        outColliders[writeOffset + colliderCount++] = chunkedCollider;
                        
                        continue;
                    }
                }

                if (y0 != y1)
                {
                    chunkedCollider.chunkIndex = (ushort) (anchorChunk + worldSize.x);
                    outColliders[writeOffset + colliderCount++] = chunkedCollider;
                }
            }

            outColliderCount[jobIndex] = colliderCount;
        }
    }
}