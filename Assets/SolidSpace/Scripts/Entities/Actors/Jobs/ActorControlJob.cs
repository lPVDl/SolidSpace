using SolidSpace.Entities.Components;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Actors
{
    [BurstCompile]
    public struct ActorControlJob : IJobParallelFor
    {
        private const float Acceleration = 100f;
        private const float RotationSpeed = FloatMath.TwoPI * 0.25f;
        private const float DesiredSpeed = Acceleration * 3;

        [ReadOnly] public NativeArray<ArchetypeChunk> inArchetypeChunks;
        [ReadOnly] public float2 inSeekPosition;
        [ReadOnly] public float inDeltaTime;
        
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<ActorComponent> actorHandle;
        
        public ComponentTypeHandle<RotationComponent> rotationHandle;
        public ComponentTypeHandle<VelocityComponent> velocityHandle;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inArchetypeChunks[chunkIndex];
            var entityCount = chunk.ChunkEntityCount;
            var positions = chunk.GetNativeArray(positionHandle);
            var velocities = chunk.GetNativeArray(velocityHandle);
            var rotations = chunk.GetNativeArray(rotationHandle);
            var actors = chunk.GetNativeArray(actorHandle);

            for (var i = 0; i < entityCount; i++)
            {
                var actor = actors[i];
                if (!actor.isActive)
                {
                    continue;
                }

                var position = positions[i];
                var velocity = velocities[i];
                var rotation = rotations[i];
                
                var targetDirection = FloatMath.Normalize(inSeekPosition - position.value);
                var targetAngle = FloatMath.Atan2(targetDirection);
                rotation.value = FloatMath.MoveAngleTowards(rotation.value, targetAngle, inDeltaTime * RotationSpeed);
                velocity.value = FloatMath.MoveTowards(velocity.value, targetDirection * DesiredSpeed, inDeltaTime * Acceleration);

                velocities[i] = velocity;
                rotations[i] = rotation;
            }
        }
    }
}