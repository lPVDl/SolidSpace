using SpaceSimulator.Entities.Rendering.Atlases;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SpaceSimulator.Entities.Rendering.Sprites
{
    [BurstCompile]
    public struct SpriteMeshComputeJob : IJob
    {
        [ReadOnly, NativeDisableContainerSafetyRestriction] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public int inFirstChunkIndex;
        [ReadOnly] public int inChunkCount;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public NativeArray<AtlasChunk> inAtlasChunks;
        [ReadOnly] public ComponentTypeHandle<SpriteRenderComponent> spriteHandle;
        [ReadOnly] public int2 inAtlasSize;

        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<SpriteVertexData> outVertices;
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<ushort> outIndices;

        private AtlasMathUtil _atlasMath;
        
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
                var renders = archetypeChunk.GetNativeArray(spriteHandle);
                var spriteCount = archetypeChunk.Count;
                
                for (var i = 0; i < spriteCount; i++)
                {
                    var position = positions[i].value;
                    var spriteRender = renders[i];
                    var spriteIndex = spriteRender.colorIndex;
                    var spriteSize = new int2(spriteRender.sizeX, spriteRender.sizeY);
                    var spriteSizeHalf = new float2(spriteSize.x * 0.5f, spriteSize.x * 0.5f);

                    var uvPixelOffset = (float2) _atlasMath.ComputeOffset(inAtlasChunks[spriteIndex.chunkId], spriteIndex);
                    var uvMin = (half2) (uvPixelOffset * pixelSize);
                    var uvMax = (half2) ((uvPixelOffset + spriteSize) * pixelSize);

                    var posMin = position - spriteSizeHalf;
                    var posMax = position + spriteSizeHalf;
                    
                    SpriteVertexData vertex;

                    vertex.position = posMin;
                    vertex.uv = uvMin;
                    outVertices[vertexOffset + 0] = vertex;

                    vertex.position.y = posMax.y;
                    vertex.uv.y = uvMax.y;
                    outVertices[vertexOffset + 1] = vertex;
                
                    vertex.position.x = posMax.x;
                    vertex.uv.x = uvMax.x;
                    outVertices[vertexOffset + 2] = vertex;

                    vertex.position.y = posMin.y;
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
    }
}