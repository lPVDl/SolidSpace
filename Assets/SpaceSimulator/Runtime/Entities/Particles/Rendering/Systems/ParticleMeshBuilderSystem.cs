using SpaceSimulator.Runtime.Entities.Common;
using SpaceSimulator.Runtime.Entities.Physics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    public class ParticleMeshBuilderSystem : SystemBase
    {
        public int ParticleCount { get; private set; }
        public NativeArray<ParticleVertexData> Vertices { get; private set; }

        private EntityQuery _query;
        private SquareVertices _square;

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
            _square = new SquareVertices
            {
                point0 = new float2(-0.5f, -0.5f),
                point1 = new float2(-0.5f, +0.5f),
                point2 = new float2(+0.5f, +0.5f),
                point3 = new float2(+0.5f, -0.5f)
            };

            Vertices = new NativeArray<ParticleVertexData>(0, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample("CreateArchetypeChunkArray");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("ComputeOffsets");
            var chunkCount = chunks.Length;
            var offsets = new NativeArray<int>(chunkCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            ParticleCount = 0;
            for (var i = 0; i < chunkCount; i++)
            {
                offsets[i] = ParticleCount * 4;
                ParticleCount += chunks[i].Count;
            }
            Profiler.EndSample();

            Profiler.BeginSample("Allocate Vertices");
            if (Vertices.Length < ParticleCount * 4)
            {
                Vertices.Dispose();
                var newCount = ParticleCount * 4;
                Vertices = new NativeArray<ParticleVertexData>(newCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("ComputeJob");
            var computeMeshJob = new ParticleComputeMeshJob
            {
                chunks = chunks,
                offsets = offsets,
                positionHandle = GetComponentTypeHandle<PositionComponent>(true),
                vertices = Vertices,
                square = _square
            };
            
            computeMeshJob.Schedule(chunks.Length, 128, Dependency).Complete();
            Profiler.EndSample();
        }

        protected override void OnDestroy()
        {
            Vertices.Dispose();
        }
    }
}