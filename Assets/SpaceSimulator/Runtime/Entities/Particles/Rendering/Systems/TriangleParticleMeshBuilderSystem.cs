using SpaceSimulator.Runtime.Entities.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    public class TriangleParticleMeshBuilderSystem : SystemBase
    {
        private const int VertexPerMesh = 65535;
        private const int TrianglePerMesh = 21845;

        public int EntityCount { get; private set; }
        public NativeArray<TriangleParticleVertexData> Vertices { get; private set; }

        private EntityQuery _query;

        protected override void OnCreate()
        {
            var queryDesc = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(PositionComponent),
                    typeof(TriangleParticleRenderComponent)
                }
            };
            
            _query = GetEntityQuery(queryDesc);
            
            Vertices = new NativeArray<TriangleParticleVertexData>(VertexPerMesh, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample("CreateArchetypeChunkArray");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("ComputeOffsets");
            var chunkCount = chunks.Length;
            var offsets = new NativeArray<int>(chunkCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            EntityCount = 0;
            for (var i = 0; i < chunkCount; i++)
            {
                offsets[i] = EntityCount;
                EntityCount += chunks[i].Count;
            }
            Profiler.EndSample();

            Profiler.BeginSample("Allocate Vertices");
            if (Vertices.Length < EntityCount * 3)
            {
                Vertices.Dispose();
                var newCount = Mathf.CeilToInt(EntityCount / (float) TrianglePerMesh) * VertexPerMesh;
                Vertices = new NativeArray<TriangleParticleVertexData>(newCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("ComputeJob");
            var unusedAmount = Vertices.Length - EntityCount * 3;
            var resetMeshJob = new FillNativeArrayJob<TriangleParticleVertexData>
            {
                array = Vertices,
                offset = Vertices.Length - unusedAmount,
                value = new TriangleParticleVertexData
                {
                    position = float2.zero
                }
            };
            var jobHandle = resetMeshJob.Schedule(unusedAmount, 128, Dependency);

            var computeMeshJob = new ComputeTriangleParticleMeshJob
            {
                chunks = chunks,
                offsets = offsets,
                positionHandle = GetComponentTypeHandle<PositionComponent>(true),
                vertices = Vertices,
            };
            
            computeMeshJob.Schedule(chunks.Length, 32, jobHandle).Complete();
            Profiler.EndSample();
        }

        protected override void OnDestroy()
        {
            Vertices.Dispose();
        }
    }
}