using SpaceSimulator.Runtime.Entities.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    public struct ComputeTriangleParticleMeshJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion, ReadOnly] public NativeArray<ArchetypeChunk> chunks;
        [DeallocateOnJobCompletion, ReadOnly] public NativeArray<int> offsets;

        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<TriangleParticleVertexData> vertices;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var entityCount = chunk.Count;

            var positions = chunk.GetNativeArray(positionHandle);
            var vec0 = new float2(-0.02f, -0.02f);
            var vec1 = new float2(0, 0.02f);
            var vec2 = new float2(0.02f, -0.02f);
            
            var vertexOffset = offsets[chunkIndex];
            for (var entityIndex = 0; entityIndex < entityCount; entityIndex++)
            {
                var position = positions[entityIndex].value;

                TriangleParticleVertexData vertex;

                vertex.position = position + vec0;
                vertices[vertexOffset] = vertex;

                vertex.position = position + vec1;
                vertices[vertexOffset + 1] = vertex;

                vertex.position = position + vec2;
                vertices[vertexOffset + 2] = vertex;
                
                vertexOffset += 3;
            }
        }
    }
}