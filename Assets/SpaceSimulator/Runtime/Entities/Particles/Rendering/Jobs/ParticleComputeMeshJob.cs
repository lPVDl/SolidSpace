using SpaceSimulator.Runtime.Entities.Common;
using SpaceSimulator.Runtime.Entities.Physics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    [BurstCompile]
    public struct ParticleComputeMeshJob : IJob
    {
        [ReadOnly] public ArchetypeChunk inChunk;
        [ReadOnly] public int inWriteOffset;
        [ReadOnly] public SquareVertices inBakedSquare;

        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;

        [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public NativeArray<ParticleVertexData> outVertices;

        [WriteOnly, NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public NativeArray<ushort> outIndices;
        
        public void Execute()
        {
            var entityCount = inChunk.Count;
            var positions = inChunk.GetNativeArray(positionHandle);
            var vertexWriteOffset = inWriteOffset * 4;
            var indexWriteOffset = inWriteOffset * 6;

            for (var i = 0; i < entityCount; i++)
            {
                var position = positions[i].value;
                
                ParticleVertexData vertex;

                vertex.position = position + inBakedSquare.point0;
                outVertices[vertexWriteOffset + 0] = vertex;
                
                vertex.position = position + inBakedSquare.point1;
                outVertices[vertexWriteOffset + 1] = vertex;
                
                vertex.position = position + inBakedSquare.point2;
                outVertices[vertexWriteOffset + 2] = vertex;
                
                vertex.position = position + inBakedSquare.point3;
                outVertices[vertexWriteOffset + 3] = vertex;

                outIndices[indexWriteOffset + 0] = (ushort) (vertexWriteOffset + 0);
                outIndices[indexWriteOffset + 1] = (ushort) (vertexWriteOffset + 1);
                outIndices[indexWriteOffset + 2] = (ushort) (vertexWriteOffset + 2);
                outIndices[indexWriteOffset + 3] = (ushort) (vertexWriteOffset + 0);
                outIndices[indexWriteOffset + 4] = (ushort) (vertexWriteOffset + 2);
                outIndices[indexWriteOffset + 5] = (ushort) (vertexWriteOffset + 3);

                vertexWriteOffset += 4;
                indexWriteOffset += 6;
            }
        }
    }
}