using SpaceSimulator.Runtime.Entities.Common;
using SpaceSimulator.Runtime.Entities.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    [BurstCompile]
    public struct ParticleComputeMeshJob : IJobParallelFor
    {
        [DeallocateOnJobCompletion, ReadOnly] public NativeArray<ArchetypeChunk> chunks;
        [DeallocateOnJobCompletion, ReadOnly] public NativeArray<int> offsets;

        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public SquareVertices square;
        
        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<ParticleVertexData> vertices;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var entityCount = chunk.Count;
            var positions = chunk.GetNativeArray(positionHandle);
            var vertexOffset = offsets[chunkIndex];
            
            for (var entityIndex = 0; entityIndex < entityCount; entityIndex++)
            {
                var position = positions[entityIndex].value;

                ParticleVertexData vertex;

                vertex.position = position + square.point0;
                vertices[vertexOffset] = vertex;

                vertex.position = position + square.point1;
                vertices[vertexOffset + 1] = vertex;

                vertex.position = position + square.point2;
                vertices[vertexOffset + 2] = vertex;
                
                vertex.position = position + square.point3;
                vertices[vertexOffset + 3] = vertex;
                
                vertexOffset += 4;
            }
        }
    }
}