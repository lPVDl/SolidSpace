using System.Collections.Generic;
using SpaceSimulator.Runtime.DebugUtils;
using SpaceSimulator.Runtime.Entities.Extensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true, OrderLast = false)]
    public class ParticleMeshRenderSystem : SystemBase
    {
        private const int RenderBounds = 8096;
        private const int ParticlePerMesh = 16384;
        private const int VertexPerMesh = ParticlePerMesh * 4;
        private const int IndexPerMesh = ParticlePerMesh * 6;
        
        public Material Material { private get; set; }

        private VertexAttributeDescriptor[] _meshLayout;
        private List<Mesh> _meshes;
        private List<int> _meshIndexCounts;
        private NativeArray<int> _indices;
        private ParticleMeshBuilderSystem _meshBuilderSystem;
        private Matrix4x4 _matrixDefault;
        private SystemBaseUtil _util;

        protected override void OnStartRunning()
        {
            _meshBuilderSystem = World.GetOrCreateSystem<ParticleMeshBuilderSystem>();
            _matrixDefault = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));

            _meshes = new List<Mesh>();
            _meshIndexCounts = new List<int>();
            _meshLayout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2),
            };

            _indices = _util.CreatePersistentArray<int>(ParticlePerMesh * 6);
            int index = 0, vertex = 0;
            for (; index < ParticlePerMesh * 6; index += 6, vertex += 4)
            {
                _indices[index + 0] = vertex + 0;
                _indices[index + 1] = vertex + 1;
                _indices[index + 2] = vertex + 2;
                _indices[index + 3] = vertex + 0;
                _indices[index + 4] = vertex + 2;
                _indices[index + 5] = vertex + 3;
            }
        }

        protected override void OnUpdate()
        {
            if (!_meshBuilderSystem.Enabled)
            {
                return;
            }
            
            var requiredMeshCount = Mathf.CeilToInt(_meshBuilderSystem.ParticleCount / (float) ParticlePerMesh);
            
            Profiler.BeginSample("CreateMesh");
            for (var i = _meshes.Count; i < requiredMeshCount; i++)
            {
                var name = nameof(ParticleMeshBuilderSystem) + "_" + i;
                _meshes.Add(CreateMesh(name));
                _meshIndexCounts.Add(0);
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("SetVertexBufferData");
            var vertexLeft = _meshBuilderSystem.ParticleCount * 4;
            var indexLeft = _meshBuilderSystem.ParticleCount * 6;
            for (var i = 0; i < requiredMeshCount; i++)
            {
                var mesh = _meshes[i];
                var vertexCount = Mathf.Min(vertexLeft, VertexPerMesh);
                mesh.SetVertexBufferData(_meshBuilderSystem.Vertices, i * VertexPerMesh, 0, vertexCount);
                
                var indexCount = Mathf.Min(indexLeft, IndexPerMesh);
                if (_meshIndexCounts[i] != indexCount)
                {
                    mesh.SetIndices(_indices, 0, indexCount, MeshTopology.Triangles, 0, false);
                    _meshIndexCounts[i] = indexCount;
                }

                vertexLeft -= vertexCount;
                indexLeft -= indexCount;
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("DrawMesh");
            for (var i = 0; i < requiredMeshCount; i++)
            {
                var renderData = new MeshDrawingData
                {
                    mesh = _meshes[i],
                    material = Material,
                    matrix = _matrixDefault,
                };
                DrawMesh(renderData);
            }
            Profiler.EndSample();
            
            SpaceDebug.LogState("ParticleCount", _meshBuilderSystem.ParticleCount);
        }

        private Mesh CreateMesh(string name)
        {
            var mesh = new Mesh
            {
                name = name,
                indexFormat = IndexFormat.UInt16,
                bounds = new Bounds(Vector3.zero, Vector3.one * RenderBounds)
            };
            mesh.MarkDynamic();
            mesh.SetVertexBufferParams(VertexPerMesh, _meshLayout);

            return mesh;
        }

        private static void DrawMesh(MeshDrawingData data)
        {
            Graphics.DrawMesh(data.mesh, data.matrix, data.material, data.layer, data.camera, data.subMeshIndex,
                data.properties, data.castShadows, data.receiveShadows, data.useLightProbes);
        }

        protected override void OnDestroy()
        {
            _indices.Dispose();
            for (var i = 0; i < _meshes.Count; i++)
            {
                Object.Destroy(_meshes[i]);
                _meshes[i] = null;
            }
        }
    }
}