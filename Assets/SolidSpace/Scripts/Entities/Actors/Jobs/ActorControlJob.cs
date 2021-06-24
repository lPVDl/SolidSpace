using System;
using SolidSpace.Debugging;
using SolidSpace.Entities.Components;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Actors
{
    [BurstCompile]
    public struct ActorControlJob : IJobParallelFor
    {
        private const float Acceleration = 30f;
        private const float RotationSpeed = FloatMath.TwoPI * 0.2f;

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
                var targetDirection = inSeekPosition - currentPosition;
                var targetDirectionNormalized = FloatMath.Normalize(targetDirection);
                var targetDistance = FloatMath.Magnitude(targetDirection);

                var currentRotation = rotations[i].value;
                FloatMath.SinCos(currentRotation, out var dirSin, out var dirCos);
                var currentDirection = new float2(dirCos, dirSin);

                var currentAngle = rotations[i].value;
                var distanceOverVelocity = targetDistance / Math.Max(1f, FloatMath.Magnitude(currentVelocity));
                var targetImpulse = targetDirectionNormalized * Acceleration * distanceOverVelocity - currentVelocity;
                var impulseAngle = FloatMath.Atan2(targetImpulse);

                var deltaAngle = FloatMath.DeltaAngle(currentAngle, impulseAngle);
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