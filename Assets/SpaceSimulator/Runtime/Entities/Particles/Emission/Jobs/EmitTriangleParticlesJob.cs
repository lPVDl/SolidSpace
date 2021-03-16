using SpaceSimulator.Runtime.Entities.Physics;
using SpaceSimulator.Runtime.Entities.Randomization;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Particles.Emission
{
    [BurstCompile]
    public struct EmitTriangleParticlesJob : IJobParallelFor
    {
        private const float TwoPI = (float)(2 * math.PI_DBL);
        
        [ReadOnly, DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> chunks;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<RandomValueComponent> randomHandle;
        [ReadOnly] public float deltaTime;
        
        public ComponentTypeHandle<TriangleParticleEmitterComponent> emitterHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<EmitParticleData> result;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var entityCount = chunk.ChunkEntityCount;
            var resultOffset = chunkIndex * chunk.Capacity;
            var chunkPositions = chunk.GetNativeArray(positionHandle);
            var chunkEmitters = chunk.GetNativeArray(emitterHandle);
            var chunkRandoms = chunk.GetNativeArray(randomHandle);
            var emitData = new EmitParticleData();

            for (var i = 0; i < entityCount; i++)
            {
                var entityEmitter = chunkEmitters[i];
                var entityPosition = chunkPositions[i];

                entityEmitter.timer += deltaTime;
                if (entityEmitter.timer >= entityEmitter.spawnDelay)
                {
                    entityEmitter.timer -= entityEmitter.spawnDelay;
                    var angle = TwoPI * chunkRandoms[i].value;
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