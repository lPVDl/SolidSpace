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
        [DeallocateOnJobCompletion, ReadOnly] public NativeArray<ArchetypeChunk> chunks;
        [DeallocateOnJobCompletion, ReadOnly] public NativeArray<int> offsets;

        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public float2 point0;
        [ReadOnly] public float2 point1;
        [ReadOnly] public float2 point2;
        
        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<TriangleParticleVertexData> vertices;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var entityCount = chunk.Count;
            var positions = chunk.GetNativeArray(positionHandle);
            var vertexOffset = offsets[chunkIndex];
            
            for (var entityIndex = 0; entityIndex < entityCount; entityIndex++)
            {
                var position = positions[entityIndex].value;

                TriangleParticleVertexData vertex;

                vertex.position = position + point0;
                vertices[vertexOffset] = vertex;

                vertex.position = position + point1;
                vertices[vertexOffset + 1] = vertex;

                vertex.position = position + point2;
                vertices[vertexOffset + 2] = vertex;
                
                vertexOffset += 3;
            }
        }
    }
}