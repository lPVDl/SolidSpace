using SpaceSimulator.Runtime.Entities.Common;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Physics.Velocity
{
    [BurstCompile]
    public struct VelocityJob : IJobParallelFor
    {
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> chunks;
        [ReadOnly] public ComponentTypeHandle<VelocityComponent> velocityHandle;
        [ReadOnly] public float deltaTime;
        
        public ComponentTypeHandle<PositionComponent> positionHandle;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var positions = chunk.GetNativeArray(positionHandle);
            var velocities = chunk.GetNativeArray(velocityHandle);
            var entityCount = chunk.Count;

            for (var i = 0; i < entityCount; i++)
            {
                var positionComponent = positions[i];
                var velocity = velocities[i].value;
                positionComponent.value += velocity * deltaTime;
                positions[i] = positionComponent;
            }
        }
    }
}