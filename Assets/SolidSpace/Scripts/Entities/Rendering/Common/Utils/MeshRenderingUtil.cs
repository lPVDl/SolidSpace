using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace SolidSpace.Entities.Rendering
{
    public struct MeshRenderingUtil
    {
        private const int EntityPerMesh = 16384;
        private const int RenderBounds = 8096;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillMesh(NativeArray<ArchetypeChunk> chunks, int startChunk, out int entityCount, out int chunkCount)
        {
            entityCount = 0;
            var chunkTotal = chunks.Length;
            var i = startChunk;
            for (; i < chunkTotal; i++)
            {
                var chunkEntityCount = chunks[i].Count;
                if (entityCount + chunkEntityCount > EntityPerMesh)
                {
                    break;
                }

                entityCount += chunkEntityCount;
            }

            chunkCount = i - startChunk;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrawMesh(MeshDrawingData data)
        {
            Graphics.DrawMesh(data.mesh, data.matrix, data.material, data.layer, data.camera, data.subMeshIndex,
                data.properties, data.castShadows, data.receiveShadows, data.useLightProbes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Mesh CreateMesh(string name)
        {
            var mesh = new Mesh
            {
                name = name,
                bounds = new Bounds(Vector3.zero, Vector3.one * RenderBounds)
            };
            mesh.MarkDynamic();

            return mesh;
        }
    }
}