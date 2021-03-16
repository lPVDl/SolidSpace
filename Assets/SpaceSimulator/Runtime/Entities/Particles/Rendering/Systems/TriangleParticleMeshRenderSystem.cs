using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
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
        
        private EntityArchetype _meshEntityArchetype;
        private VertexAttributeDescriptor[] _meshLayout;
        private List<ParticleMeshEntityData> _meshes;
        private NativeArray<int> _indicesDefault;
        private LocalToWorld _localToWorldDefault;
        private RenderBounds _renderBoundsDefault;
        private TriangleParticleMeshBuilderSystem _meshBuilderSystem;

        protected override void OnStartRunning()
        {
            _meshBuilderSystem = World.GetOrCreateSystem<TriangleParticleMeshBuilderSystem>();
            
            _meshes = new List<ParticleMeshEntityData>();
            _meshLayout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2)
            };
            _meshEntityArchetype = EntityManager.CreateArchetype(new ComponentType[]
            {
                typeof(LocalToWorld),
                typeof(RenderMesh),
                typeof(RenderBounds),
            });
            _localToWorldDefault = new LocalToWorld
            {
                Value = float4x4.identity
            };
            _renderBoundsDefault = new RenderBounds
            {
                Value = new AABB
                {
                    Center = float3.zero,
                    Extents = new float3(RenderBounds, RenderBounds, 0.1f)
                }
            };
            
            _indicesDefault = new NativeArray<int>(VertexPerMesh, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < VertexPerMesh; i++)
                _indicesDefault[i] = i;
        }

        protected override void OnUpdate()
        {
            UpdateMeshCount();
            UpdateMeshTopology();
        }
        
        private void UpdateMeshCount()
        {
            Profiler.BeginSample(nameof(UpdateMeshCount));
            
            var requiredMeshCount = Mathf.CeilToInt(_meshBuilderSystem.EntityCount / (float) TrianglePerMesh);
            var meshCountMax = Mathf.Max(requiredMeshCount, _meshes.Count);
            for (var i = 0; i < meshCountMax; i++)
            {
                ParticleMeshEntityData meshData;
                
                if (i >= _meshes.Count)
                {
                    var name = nameof(TriangleParticleMeshBuilderSystem) + "_" + i;
                    meshData = new ParticleMeshEntityData
                    {
                        entity = EntityManager.CreateEntity(_meshEntityArchetype),
                        mesh = CreateMesh(name),
                        visible = false
                    };
                    _meshes.Add(meshData);
                    EntityManager.SetName(meshData.entity, name);
                    EntityManager.SetComponentData(meshData.entity, _localToWorldDefault);
                    EntityManager.SetComponentData(meshData.entity, _renderBoundsDefault);
                }
                else
                {
                    meshData = _meshes[i];
                }

                var shouldDisplayMesh = i < requiredMeshCount;
                if (shouldDisplayMesh == meshData.visible)
                {
                    continue;
                }

                meshData.visible = shouldDisplayMesh;
                var mesh = shouldDisplayMesh ? meshData.mesh : null;
                EntityManager.SetSharedComponentData(meshData.entity, CreateRenderMesh(mesh));
                _meshes[i] = meshData;
            }

            Profiler.EndSample();
        }
        
        private void UpdateMeshTopology()
        {
            Profiler.BeginSample(nameof(UpdateMeshTopology));
            
            var usedMeshCount = Mathf.CeilToInt(_meshBuilderSystem.EntityCount * 3f / VertexPerMesh);
            for (var i = 0; i < usedMeshCount; i++)
            {
                _meshes[i].mesh.SetVertexBufferData(_meshBuilderSystem.Vertices, i * VertexPerMesh, 0, VertexPerMesh);
            }
            
            Profiler.EndSample();
        }
        
        private Mesh CreateMesh(string name)
        {
            var mesh = new Mesh();
            mesh.name = name;
            mesh.indexFormat = IndexFormat.UInt16;
            mesh.MarkDynamic();
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * RenderBounds);
            mesh.SetVertexBufferParams(VertexPerMesh, _meshLayout);
            mesh.SetIndices(_indicesDefault, MeshTopology.Triangles, 0, false);

            return mesh;
        }

        private RenderMesh CreateRenderMesh(Mesh mesh)
        {
            return new RenderMesh
            {
                material = Material,
                castShadows = ShadowCastingMode.Off,
                receiveShadows = false,
                needMotionVectorPass = false,
                mesh = mesh
            };
        }

        protected override void OnDestroy()
        {
            _indicesDefault.Dispose();
            for (var i = 0; i < _meshes.Count; i++)
            {
                Object.Destroy(_meshes[i].mesh);
            }
        }
    }
}