using SpaceSimulator.Runtime.Entities.Common;
using SpaceSimulator.Runtime.Entities.Extensions;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    public class ParticleMeshBuilderSystem : SystemBase
    {
        private const int VerticesChunkSize = 65536;
        
        public int ParticleCount { get; private set; }
        public NativeArray<ParticleVertexData> Vertices => _vertices;

        private EntityQuery _query;
        private SquareVertices _square;
        private SystemBaseUtil _util;
        private NativeArray<ParticleVertexData> _vertices;

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
            _vertices = _util.CreatePersistentArray<ParticleVertexData>(0);
        }

        protected override void OnUpdate()
        {
            Profiler.BeginSample("CreateArchetypeChunkArray");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();

            Profiler.BeginSample("ComputeOffsets");
            var chunkCount = chunks.Length;
            var offsets = _util.CreateTempJobArray<int>(chunkCount);
            ParticleCount = 0;
            for (var i = 0; i < chunkCount; i++)
            {
                offsets[i] = ParticleCount * 4;
                ParticleCount += chunks[i].Count;
            }
            Profiler.EndSample();
            
            _util.MaintainPersistentArrayLength(ref _vertices, ParticleCount * 4, VerticesChunkSize);

            Profiler.BeginSample("ComputeJob");
            var computeMeshJob = new ParticleComputeMeshJob
            {
                chunks = chunks,
                offsets = offsets,
                positionHandle = GetComponentTypeHandle<PositionComponent>(true),
                vertices = _vertices,
                square = _square
            };
            
            computeMeshJob.Schedule(chunks.Length, 128, Dependency).Complete();
            Profiler.EndSample();

            chunks.Dispose();
            offsets.Dispose();
        }

        protected override void OnDestroy()
        {
            _vertices.Dispose();
        }
    }
}