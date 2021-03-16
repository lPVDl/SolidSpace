using System.Collections.Generic;
using SpaceMassiveSimulator.Runtime.Entities.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace SpaceMassiveSimulator.Runtime.Entities.Particles
{
    public class TriangleParticleRenderSystem : SystemBase
    {
        public Material Material { private get; set; }
        
        private const int TrianglePerMesh = 21845;
        private const int VertexPerMesh = 65535;
        private const float RenderBounds = 1000;

        private NativeArray<TriangleParticleVertexData> _vertices;
        private int _entityCount;
        private EntityQuery _query;
        private VertexAttributeDescriptor[] _meshLayout;
        private List<ParticleMeshEntityData> _meshes;
        private NativeArray<int> _indexStandard;
        private EntityArchetype _meshEntityArchetype;
        private EndSimulationEntityCommandBufferSystem _commandBufferSystem;

        protected override void OnCreate()
        {
            _commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _meshLayout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2)
            };
            var components = new ComponentType[]
            {
                typeof(LocalToWorld),
                typeof(RenderMesh),
                typeof(RenderBounds),
            };
            _meshEntityArchetype = EntityManager.CreateArchetype(components);
            _meshes = new List<ParticleMeshEntityData>();

            var queryDesc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PositionComponent),
                    typeof(TriangleParticleRenderComponent)
                }
            };
            _query = GetEntityQuery(queryDesc);
            _vertices = new NativeArray<TriangleParticleVertexData>(VertexPerMesh, Allocator.Persistent);
            _indexStandard = new NativeArray<int>(VertexPerMesh, Allocator.Persistent);
            for (var i = 0; i < VertexPerMesh; i++)
                _indexStandard[i] = i;
        }

        protected override void OnUpdate()
        {
            UpdateMeshCount();
            UpdateMeshTopology();
            var chunks = UpdateComputationArray();
            ScheduleJobs(chunks);
        }

        private void UpdateMeshCount()
        {
            Profiler.BeginSample(nameof(UpdateMeshCount));

            var commandBufferCreated = false;
            EntityCommandBuffer commandBuffer = default;
            var requiredMeshCount = Mathf.CeilToInt(_entityCount / (float) TrianglePerMesh);
            var meshCountMax = Mathf.Max(requiredMeshCount, _meshes.Count);
            for (var i = 0; i < meshCountMax; i++)
            {
                ParticleMeshEntityData meshData;
                
                if (i >= _meshes.Count)
                {
                    var name = nameof(TriangleParticleRenderSystem) + "_" + i;
                    meshData = new ParticleMeshEntityData
                    {
                        entity = EntityManager.CreateEntity(_meshEntityArchetype),
                        mesh = CreateMesh(name),
                        visible = true
                    };
                    _meshes.Add(meshData);
                    EntityManager.SetName(meshData.entity, name);
                    EntityManager.SetSharedComponentData(meshData.entity, CreateRenderMesh(meshData.mesh));
                    EntityManager.SetComponentData(meshData.entity, new LocalToWorld
                    {
                        Value = float4x4.identity
                    });
                    EntityManager.SetComponentData(meshData.entity, new RenderBounds
                    {
                        Value = new AABB
                        {
                            Center = float3.zero,
                            Extents = new float3(RenderBounds, RenderBounds, 0.1f)
                        }
                    });
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
                
                if (!commandBufferCreated)
                {
                    commandBuffer = _commandBufferSystem.CreateCommandBuffer();
                    commandBufferCreated = true;
                }

                meshData.visible = shouldDisplayMesh;
                SetMeshEntityRenderData(commandBuffer, meshData.entity, shouldDisplayMesh ? meshData.mesh : null);
                _meshes[i] = meshData;
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
            mesh.SetIndices(_indexStandard, MeshTopology.Triangles, 0, false);

            return mesh;
        }

        private void SetMeshEntityRenderData(EntityCommandBuffer commandBuffer, Entity entity, Mesh mesh)
        {
            commandBuffer.SetSharedComponent(entity, new RenderMesh
            {
                material = Material,
                castShadows = ShadowCastingMode.Off,
                receiveShadows = false,
                needMotionVectorPass = false,
                mesh = mesh
            });
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
        
        private void UpdateMeshTopology()
        {
            Profiler.BeginSample(nameof(UpdateMeshTopology));

            var usedMeshCount = Mathf.CeilToInt(_entityCount * 3f / VertexPerMesh);
            for (var i = 0; i < usedMeshCount; i++)
            {
                _meshes[i].mesh.SetVertexBufferData(_vertices, i * VertexPerMesh, 0, VertexPerMesh);
            }
            
            Profiler.EndSample();
        }

        private NativeArray<ArchetypeChunk> UpdateComputationArray()
        {
            Profiler.BeginSample(nameof(UpdateComputationArray));
            
            Profiler.BeginSample("CreateArchetypeChunkArray");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("CalculateEntityCount");
            _entityCount = _query.CalculateEntityCount();
            Profiler.EndSample();

            Profiler.BeginSample("Allocate _vertices");
            if (_vertices.Length < _entityCount * 3)
            {
                _vertices.Dispose();
                var newCount = Mathf.CeilToInt(_entityCount / (float) TrianglePerMesh) * VertexPerMesh;
                _vertices = new NativeArray<TriangleParticleVertexData>(newCount, Allocator.Persistent);
            }

            Profiler.EndSample();
            
            Profiler.EndSample();

            return chunks;
        }

        private void ScheduleJobs(NativeArray<ArchetypeChunk> chunks)
        {
            Profiler.BeginSample(nameof(ScheduleJobs));
            
            var unusedAmount = _vertices.Length - _entityCount * 3;
            var resetMeshJob = new FillNativeArrayJob<TriangleParticleVertexData>
            {
                array = _vertices,
                offset = _vertices.Length - unusedAmount,
                value = new TriangleParticleVertexData
                {
                    position = float2.zero
                }
            };
            var jobHandle = resetMeshJob.Schedule(unusedAmount, 128, Dependency);

            var computeMeshJob = new ComputeTriangleParticleMeshJob
            {
                chunks = chunks,
                positionHandle = GetComponentTypeHandle<PositionComponent>(true),
                vertices = _vertices,
            };
            
            Dependency = computeMeshJob.Schedule(chunks.Length, 32, jobHandle);
            
            Profiler.EndSample();
        }

        protected override void OnDestroy()
        {
            _vertices.Dispose();
            _indexStandard.Dispose();
            for (var i = 0; i < _meshes.Count; i++)
            {
                Object.Destroy(_meshes[i].mesh);
            }
        }
    }
}