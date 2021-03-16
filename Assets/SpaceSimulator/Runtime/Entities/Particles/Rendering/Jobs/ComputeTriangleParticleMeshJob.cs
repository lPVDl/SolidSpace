using SpaceSimulator.Runtime.Entities.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    [BurstCompile]
    public struct ComputeTriangleParticleMeshJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion] 
        public NativeArray<ArchetypeChunk> chunks;

        [WriteOnly][NativeDisableParallelForRestriction] 
        public NativeArray<TriangleParticleVertexData> vertices;

        [ReadOnly] 
        public ComponentTypeHandle<PositionComponent> positionHandle;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var instanceCount = chunk.Count;

            var chunkPositions = chunk.GetNativeArray(positionHandle);
            var chunkVertexOffset = chunkIndex * chunk.Capacity * 3;
            var vec0 = new float2(-0.02f, -0.02f);
            var vec1 = new float2(0, 0.02f);
            var vec2 = new float2(0.02f, -0.02f);
                
            for (var i = 0; i < instanceCount; i++)
            {
                var instancePosition = chunkPositions[i].value;
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