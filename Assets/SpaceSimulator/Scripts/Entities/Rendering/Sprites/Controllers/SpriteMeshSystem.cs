using System.Collections.Generic;
using SpaceSimulator.DebugUtils;
using SpaceSimulator.Entities.Rendering.Particles;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace SpaceSimulator.Entities.Rendering.Sprites
{
    public class SpriteMeshSystem : IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Render;
        
        private readonly IEntityManager _entityManager;
        private readonly SpriteMeshSystemConfig _config;

        private EntityQuery _query;
        private SquareVertices _square;
        private NativeArrayUtil _arrayUtil;
        private MeshRenderingUtil _meshUtil;
        private VertexAttributeDescriptor[] _meshLayout;
        private List<Mesh> _meshes;
        private List<Mesh> _meshesForMeshArray;
        private Matrix4x4 _matrixDefault;
        private MeshUpdateFlags _meshUpdateFlags;
        private Material _material;

        public SpriteMeshSystem(IEntityManager entityManager, SpriteMeshSystemConfig config)
        {
            _entityManager = entityManager;
            _config = config;
        }
        
        public void Initialize()
        {
            _material = new Material(_config.Shader);
            _matrixDefault = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            _meshes = new List<Mesh>();
            _meshesForMeshArray = new List<Mesh>();
            _meshUpdateFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;
            _meshLayout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2),
            };
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(SpriteRenderComponent)
            });
            _square = new SquareVertices
            {
                point0 = new float2(-0.5f, -0.5f),
                point1 = new float2(-0.5f, +0.5f),
                point2 = new float2(+0.5f, +0.5f),
                point3 = new float2(+0.5f, -0.5f)
            };
        }

        public void Update()
        {
            Profiler.BeginSample("Query chunks");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            Profiler.EndSample();
            
            Profiler.BeginSample("Compute offsets");
            var chunkTotal = chunks.Length;
            var chunkPerMesh = _arrayUtil.CreateTempJobArray<int>(chunkTotal);
            var spritePerMesh = _arrayUtil.CreateTempJobArray<int>(chunkTotal);
            var totalSpriteCount = 0;
            var meshCount = 0;
            var chunkIndex = 0;
            while (chunkIndex < chunkTotal)
            {
                _meshUtil.FillMesh(chunks, chunkIndex, out var spriteCount, out var chunkCount);
                chunkPerMesh[meshCount] = chunkCount;
                spritePerMesh[meshCount] = spriteCount;
                totalSpriteCount += spriteCount;
                chunkIndex += chunkCount;
                meshCount++;
            }
            Profiler.EndSample();

            Profiler.BeginSample("Compute meshes");
            var meshDataArray = Mesh.AllocateWritableMeshData(meshCount);
            var computeJobHandles = _arrayUtil.CreateTempJobArray<JobHandle>(meshCount);
            var positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true);
            var chunkOffset = 0;
            for (var i = 0; i < meshCount; i++)
            {
                var meshData = meshDataArray[i];
                var spriteCount = spritePerMesh[i];
                meshData.SetVertexBufferParams(spriteCount * 4, _meshLayout);
                meshData.SetIndexBufferParams(spriteCount * 6, IndexFormat.UInt16);
                meshData.subMeshCount = 1;
                var subMeshDescriptor = new SubMeshDescriptor(0, spriteCount * 6);
                meshData.SetSubMesh(0, subMeshDescriptor, _meshUpdateFlags);

                var meshChunkCount = chunkPerMesh[i];
                var job = new SpriteMeshComputeJob
                {
                    inChunks = chunks,
                    positionHandle = positionHandle,
                    inBakedSquare = _square,
                    inChunkCount = meshChunkCount,
                    inFirstChunkIndex = chunkOffset,
                    outIndices = meshData.GetIndexData<ushort>(),
                    outVertices = meshData.GetVertexData<SpriteVertexData>()
                };
                computeJobHandles[i] = job.Schedule();

                chunkOffset += meshChunkCount;
            }

            var computeHandle = JobHandle.CombineDependencies(computeJobHandles);
            computeHandle.Complete();
            Profiler.EndSample();

            Profiler.BeginSample("Create meshes");
            for (var i = _meshes.Count; i < meshCount; i++)
            {
                var name = nameof(SpriteMeshSystem) + "_" + i;
                _meshes.Add(_meshUtil.CreateMesh(name));
            }
            _meshesForMeshArray.Clear();
            for (var i = 0; i < meshCount; i++)
            {
                _meshesForMeshArray.Add(_meshes[i]);
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("Apply and dispose writable mesh data");
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _meshesForMeshArray, _meshUpdateFlags);
            Profiler.EndSample();
            
            Profiler.BeginSample("Draw mesh");
            for (var i = 0; i < meshCount; i++)
            {
                _meshUtil.DrawMesh(new MeshDrawingData
                {
                    mesh = _meshes[i],
                    material = _material,
                    matrix = _matrixDefault
                });
            }
            Profiler.EndSample();
            
            Profiler.BeginSample("Dispose native arrays");
            chunks.Dispose();
            chunkPerMesh.Dispose();
            spritePerMesh.Dispose();
            computeJobHandles.Dispose();
            Profiler.EndSample();
            
            SpaceDebug.LogState("SpriteCount", totalSpriteCount);
            SpaceDebug.LogState("SpriteMeshCount", meshCount);
        }

        public void FinalizeSystem()
        {
            for (var i = 0; i < _meshes.Count; i++)
            {
                Object.Destroy(_meshes[i]);
                _meshes[i] = null;
            }
            Object.Destroy(_material);
        }
    }
}