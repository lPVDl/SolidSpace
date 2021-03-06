using System.Runtime.CompilerServices;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Components;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Rendering.Sprites
{
    [BurstCompile]
    internal struct SpriteMeshComputeJob : IJob
    {
        private struct Square
        {
            public float2 center;
            public float2 size;
            public float rotation;
            public half2 uvMin;
            public half2 uvMax;
        }
        
        [ReadOnly, NativeDisableContainerSafetyRestriction] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public int inFirstChunkIndex;
        [ReadOnly] public int inChunkCount;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<SpriteRenderComponent> spriteHandle;
        [ReadOnly] public ComponentTypeHandle<RotationComponent> rotationHandle;
        [ReadOnly] public NativeSlice<AtlasChunk2D> inAtlasChunks;
        [ReadOnly] public ComponentTypeHandle<RectSizeComponent> sizeHandle;
        [ReadOnly] public int2 inAtlasSize;

        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<SpriteVertexData> outVertices;
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<ushort> outIndices;
        
        public void Execute()
        {
            var lastChunkIndex = inFirstChunkIndex + inChunkCount;
            var vertexOffset = 0;
            var indexOffset = 0;
            var pixelSize = new float2(1f / inAtlasSize.x, 1f / inAtlasSize.y);
            
            for (var chunkIndex = inFirstChunkIndex; chunkIndex < lastChunkIndex; chunkIndex++)
            {
                var archetypeChunk = inChunks[chunkIndex];
                var positions = archetypeChunk.GetNativeArray(positionHandle);
                var sizes = archetypeChunk.GetNativeArray(sizeHandle);
                var renders = archetypeChunk.GetNativeArray(spriteHandle);
                var spriteCount = archetypeChunk.Count;
                
                var hasRotation = false;
                NativeArray<RotationComponent> rotations = default;
                
                if (archetypeChunk.Has(rotationHandle))
                {
                    rotations = archetypeChunk.GetNativeArray(rotationHandle);
                    hasRotation = true;
                }

                for (var i = 0; i < spriteCount; i++)
                {
                    var renderIndex = renders[i].index;
                    var renderChunk = inAtlasChunks[renderIndex.chunkId];
                    var size = sizes[i].value;
                    var uvPixelOffset = (float2) AtlasMath.ComputeOffset(renderChunk, renderIndex);
                    
                    FlushSquare(ref indexOffset, ref vertexOffset, new Square
                    {
                        uvMin = (half2) (uvPixelOffset * pixelSize),
                        uvMax = (half2) ((uvPixelOffset + size) * pixelSize),
                        center = positions[i].value,
                        size = size,
                        rotation = hasRotation ? rotations[i].value : 0f
                    });
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushSquare(ref int indexOffset, ref int vertexOffset, Square square)
        {
            SpriteVertexData vertex;
            var halfSize = square.size * 0.5f;
            var uvMin = square.uvMin;
            var uvMax = square.uvMax;
            FloatMath.SinCos(square.rotation, out var sin, out var cos);

            vertex.position = square.center + FloatMath.Rotate(-halfSize.x, -halfSize.y, sin, cos);
            vertex.uv = uvMin;
            outVertices[vertexOffset + 0] = vertex;

            vertex.position = square.center + FloatMath.Rotate(-halfSize.x, +halfSize.y, sin, cos);
            vertex.uv.y = uvMax.y;
            outVertices[vertexOffset + 1] = vertex;
                
            vertex.position = square.center + FloatMath.Rotate(+halfSize.x, +halfSize.y, sin, cos);
            vertex.uv.x = uvMax.x;
            outVertices[vertexOffset + 2] = vertex;

            vertex.position = square.center + FloatMath.Rotate(+halfSize.x, -halfSize.y, sin, cos);
            vertex.uv.y = uvMin.y;
            outVertices[vertexOffset + 3] = vertex;

            outIndices[indexOffset + 0] = (ushort) (vertexOffset + 0);
            outIndices[indexOffset + 1] = (ushort) (vertexOffset + 1);
            outIndices[indexOffset + 2] = (ushort) (vertexOffset + 2);
            outIndices[indexOffset + 3] = (ushort) (vertexOffset + 0);
            outIndices[indexOffset + 4] = (ushort) (vertexOffset + 2);
            outIndices[indexOffset + 5] = (ushort) (vertexOffset + 3);
            
            vertexOffset += 4;
            indexOffset += 6;
        }
    }
}