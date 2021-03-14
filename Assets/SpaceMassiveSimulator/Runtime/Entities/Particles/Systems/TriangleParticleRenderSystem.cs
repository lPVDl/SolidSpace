using System.Collections.Generic;
using SpaceMassiveSimulator.Runtime.Entities.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace SpaceMassiveSimulator.Runtime.Entities.Particles
{
    public class TriangleParticleRenderSystem : SystemBase
    {
        private const int TrianglePerMesh = 21845;
        private const int VertexPerMesh = 65535;

        public IReadOnlyList<Mesh> Meshes => _meshes;

        private NativeArray<TriangleParticleVertexData> _vertices;
        private NativeArray<int> _indices;
        private int _entityCount;
        private EntityQuery _query;
        private VertexAttributeDescriptor[] _meshLayout;
        private List<Mesh> _meshes;

        private NativeArray<int> _indexStandart;

        protected override void OnCreate()
        {
            _meshLayout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2)
            };

            var queryDesc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PositionComponent)
                }
            };
            _query = GetEntityQuery(queryDesc);
            _vertices = new NativeArray<TriangleParticleVertexData>(VertexPerMesh, Allocator.Persistent);
            _meshes = new List<Mesh>();
            _indexStandart = new NativeArray<int>(VertexPerMesh, Allocator.Persistent);
            for (var i = 0; i < VertexPerMesh; i++)
                _indexStandart[i] = i;
        }

        protected override void OnUpdate()
        {
            var requiredMeshCount = Mathf.CeilToInt(_entityCount / (float) TrianglePerMesh);
            var newMeshCount = requiredMeshCount - _meshes.Count;
            for (var i = 0; i < newMeshCount; i++)
            {
                var mesh = new Mesh();
                mesh.MarkDynamic();
                mesh.SetVertexBufferParams(VertexPerMesh, _meshLayout);
                mesh.indexFormat = IndexFormat.UInt16;
                mesh.SetIndices(_indexStandart, MeshTopology.Triangles, 0, false);
                _meshes.Add(mesh);
            }

            Profiler.BeginSample("Mesh.SetVertexBufferData");
            for (var i = 0; i < requiredMeshCount; i++)
            {
                _meshes[i].SetVertexBufferData(_vertices, i * VertexPerMesh, 0, VertexPerMesh);
            }

            Profiler.EndSample();

            Profiler.BeginSample("_query.CreateArchetypeChunkArray");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("Compute Entity Count");
            _entityCount = 0;
            for (var i = 0; i < chunks.Length; i++)
            {
                _entityCount += chunks[i].ChunkEntityCount;
            }

            Profiler.EndSample();

            Profiler.BeginSample("Allocate _vertices");
            if (_vertices.Length < _entityCount * 3)
            {
                _vertices.Dispose();
                var newCount = Mathf.CeilToInt(_entityCount / (float) TrianglePerMesh) * VertexPerMesh;
                _vertices = new NativeArray<TriangleParticleVertexData>(newCount, Allocator.Persistent);
            }

            Profiler.EndSample();

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
        }

        protected override void OnDestroy()
        {
            _vertices.Dispose();
            _indexStandart.Dispose();
        }
    }
}