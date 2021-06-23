using System;
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
        private const float Acceleration = 30f;
        private const float RotationSpeed = FloatMath.TwoPI * 0.1f;
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

                var currentPosition = positions[i].value;
                var currentVelocity = velocities[i].value;
                var currentAngle = rotations[i].value;
                var targetDirection = FloatMath.Normalize(inSeekPosition - currentPosition);
                var targetImpulse = targetDirection * DesiredSpeed - currentVelocity;
                var impulseAngle = FloatMath.Atan2(targetImpulse);
                var deltaAngle = FloatMath.DeltaAngle(currentAngle, impulseAngle);
                FloatMath.SinCos(currentAngle, out var sin, out var cos);
                var currentDirection = new float2(cos, sin);
                var thrusterPower = 1 - Math.Min(1, Math.Abs(deltaAngle) / (FloatMath.PI * 0.5f));
                var impulse = currentDirection * (inDeltaTime * thrusterPower * Acceleration);

                velocities[i] = new VelocityComponent
                {
                    value = currentVelocity + impulse
                };

                rotations[i] = new RotationComponent
                {
                    value = FloatMath.MoveAngleTowards(currentAngle, impulseAngle, inDeltaTime * RotationSpeed)
                };
            }
        }
    }
}