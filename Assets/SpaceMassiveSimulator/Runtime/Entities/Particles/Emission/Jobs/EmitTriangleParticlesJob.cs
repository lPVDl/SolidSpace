using SpaceMassiveSimulator.Runtime.Entities.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SpaceMassiveSimulator.Runtime.Entities.Particles.Emission
{
    [BurstCompile]
    public struct EmitTriangleParticlesJob : IJobParallelFor
    {
        private const float TwoPI = (float)(2 * math.PI_DBL);
        
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> chunks;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        public ComponentTypeHandle<TriangleParticleEmitterComponent> emitterHandle;
        [ReadOnly] public NativeArray<float> randomValues;
        [ReadOnly] public int randomIndex;
        [ReadOnly] public float deltaTime;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<EmitParticleData> result;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var entityCount = chunk.ChunkEntityCount;
            var resultOffset = chunkIndex * chunk.Capacity;
            var randomOffset = resultOffset + randomIndex;
            var randomCount = randomValues.Length;
            var chunkPositions = chunk.GetNativeArray(positionHandle);
            var chunkEmitters = chunk.GetNativeArray(emitterHandle);
            var emitData = new EmitParticleData();

            for (var i = 0; i < entityCount; i++)
            {
                var entityEmitter = chunkEmitters[i];
                var entityPosition = chunkPositions[i];

                entityEmitter.timer += deltaTime;
                if (entityEmitter.timer >= entityEmitter.spawnDelay)
                {
                    entityEmitter.timer -= entityEmitter.spawnDelay;
                    var angle = TwoPI * randomValues[(randomOffset + i) % randomCount];
                    emitData.position = entityPosition.value;
                    emitData.velocity = new float2(math.cos(angle), math.sin(angle));
                    emitData.emit = true;
                }
                else
                {
                    emitData.emit = false;
                }

                result[resultOffset + i] = emitData;
                chunkEmitters[i] = entityEmitter;
            }
        }
    }
}