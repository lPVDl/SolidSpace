using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceMassiveSimulator.Runtime.Entities.Particles
{
    [BurstCompile]
    public struct ComputeTriangleParticleMeshJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> chunks;

        [WriteOnly][NativeDisableParallelForRestriction] 
        public NativeArray<TriangleParticleVertexData> vertices;

        [ReadOnly] public ComponentTypeHandle<Translation> translationHandle;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var instanceCount = chunk.Count;

            var chunkTranslation = chunk.GetNativeArray(translationHandle);
            var chunkVertexOffset = chunkIndex * chunk.Capacity * 3;
            var vec0 = new float3(-0.02f, -0.02f, 0);
            var vec1 = new float3(0, 0.02f, 0);
            var vec2 = new float3(0.02f, -0.02f, 0);
                
            for (var i = 0; i < instanceCount; i++)
            {
                var instancePosition = chunkTranslation[i].Value;
                var instanceVertexGlobalOffset = chunkVertexOffset + i * 3;

                TriangleParticleVertexData vertex;

                vertex.position = instancePosition + vec0;
                vertices[instanceVertexGlobalOffset] = vertex;

                vertex.position = instancePosition + vec1;
                vertices[instanceVertexGlobalOffset + 1] = vertex;

                vertex.position = instancePosition + vec2;
                vertices[instanceVertexGlobalOffset + 2] = vertex;
            }
        }
    }
}