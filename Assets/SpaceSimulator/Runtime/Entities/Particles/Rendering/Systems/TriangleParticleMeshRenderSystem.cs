using System.Collections.Generic;
using SpaceSimulator.Runtime.DebugUtils;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    [UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true, OrderLast = false)]
    public class TriangleParticleMeshRenderSystem : SystemBase
    {
        private const int VertexPerMesh = 65535;
        private const int TrianglePerMesh = 21845;
        private const float RenderBounds = 1000;
        
        public Material Material { private get; set; }

        private VertexAttributeDescriptor[] _meshLayout;
        private List<Mesh> _meshes;
        private NativeArray<int> _indicesDefault;
        private TriangleParticleMeshBuilderSystem _meshBuilderSystem;
        private Matrix4x4 _matrixDefault;

        protected override void OnStartRunning()
        {
            _meshBuilderSystem = World.GetOrCreateSystem<TriangleParticleMeshBuilderSystem>();
            _matrixDefault = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));

            _meshes = new List<Mesh>();
            _meshLayout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2),
            };

            _indicesDefault = new NativeArray<int>(VertexPerMesh, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            var minusForward = -Vector3.forward;
            for (var i = 0; i < VertexPerMesh; i++)
            {
                _indicesDefault[i] = i;
            }
        }

        protected override void OnUpdate()
        {
            if (!_meshBuilderSystem.Enabled)
            {
                return;
            }
            
            var requiredMeshCount = Mathf.CeilToInt(_meshBuilderSystem.EntityCount / (float) TrianglePerMesh);
            
            Profiler.BeginSample("CreateMesh");
            for (var i = _meshes.Count; i < requiredMeshCount; i++)
            {
                var name = nameof(TriangleParticleMeshBuilderSystem) + "_" + i;
                _meshes.Add(CreateMesh(name));
            }
            Profiler.EndSample();

            Profiler.BeginSample("SetVertexBufferData");
            for (var i = 0; i < requiredMeshCount; i++)
            {
                _meshes[i].SetVertexBufferData(_meshBuilderSystem.Vertices, i * VertexPerMesh, 0, VertexPerMesh);
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("DrawMesh");
            for (var i = 0; i < requiredMeshCount - 1; i++)
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
            
            SpaceDebug.LogState("ParticleCount", _meshBuilderSystem.EntityCount);
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
            mesh.SetIndices(_indicesDefault, MeshTopology.Triangles, 0);

            return mesh;
        }

        private static void DrawMesh(MeshDrawingData data)
        {
            Graphics.DrawMesh(data.mesh, data.matrix, data.material, data.layer, data.camera, data.subMeshIndex,
                data.properties, data.castShadows, data.receiveShadows, data.useLightProbes);
        }

        protected override void OnDestroy()
        {
            _indicesDefault.Dispose();
            for (var i = 0; i < _meshes.Count; i++)
            {
                Object.Destroy(_meshes[i]);
                _meshes[i] = null;
            }
        }
    }
}