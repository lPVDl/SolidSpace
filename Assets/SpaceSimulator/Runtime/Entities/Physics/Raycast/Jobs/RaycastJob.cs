using System.Runtime.CompilerServices;
using SpaceSimulator.Runtime.Entities.Common;
using SpaceSimulator.Runtime.Entities.Physics.Velocity;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    [BurstCompile]
    public struct RaycastJob : IJobParallelFor
    {
        [ReadOnly] public ColliderWorld inColliderWorld;
        [ReadOnly] public NativeArray<ArchetypeChunk> raycasterChunks;
        [ReadOnly] public NativeArray<int> resultWriteOffsets;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<VelocityComponent> velocityHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;
        [ReadOnly] public float deltaTime;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Entity> resultEntities;
        [WriteOnly] public NativeArray<int> resultCounts;
        public void Execute(int chunkIndex)
        {
            var chunk = raycasterChunks[chunkIndex];
            var rayCount = chunk.Count;
            var positions = chunk.GetNativeArray(positionHandle);
            var velocities = chunk.GetNativeArray(velocityHandle);
            var entities = chunk.GetNativeArray(entityHandle);
            var writeOffset = resultWriteOffsets[chunkIndex];
            var hitCount = 0;
            var worldPower = inColliderWorld.worldGrid.power;
            var worldAnchor = inColliderWorld.worldGrid.anchor;
            var worldSize = inColliderWorld.worldGrid.size;
            
            for (var i = 0; i < rayCount; i++)
            {
                var pos = positions[i].value;

                var x0 = ((int) pos.x >> worldPower) - worldAnchor.x;
                if (x0 < 0 || x0 >= worldSize.x)
                {
                    continue;
                }
                
                var y0 = ((int) pos.y >> worldPower) - worldAnchor.y;
                if (y0 < 0 || y0 >= worldSize.y)
                {
                    continue;
                }

                var rayCellIndex = y0 * worldSize.x + x0;
                var rayCell = inColliderWorld.worldCells[rayCellIndex];
                if (rayCell.count == 0)
                {
                    continue;
                }

                for (var j = 0; j < rayCell.count; j++)
                {
                    var colliderIndex = inColliderWorld.colliderStream[rayCell.offset + j];
                    var collider = inColliderWorld.colliders[colliderIndex];

                    if (!BoundsOverlap(pos.x, pos.x, collider.xMin, collider.xMax))
                    {
                        continue;
                    }

                    if (!BoundsOverlap(pos.y, pos.y, collider.yMin, collider.yMax))
                    {
                        continue;
                    }

                    resultEntities[writeOffset + hitCount++] = entities[i];
                    
                    break;
                }
            }

            resultCounts[chunkIndex] = hitCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MinMax(float a, float b, out float min, out float max)
        {
            if (a > b)
            {
                min = b;
                max = a;
            }
            else
            {
                min = a;
                max = b;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool BoundsOverlap(float min0, float max0, float min1, float max1)
        {
            return (max1 >= min0) && (max0 >= min1);
        }
    }
}