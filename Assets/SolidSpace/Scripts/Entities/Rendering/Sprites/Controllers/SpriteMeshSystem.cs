using System.Collections.Generic;
using SolidSpace.Debugging;
using SolidSpace.GameCycle;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public class SpriteMeshSystem : IController
    {
        private static readonly int MainTexturePropertyId = Shader.PropertyToID("_MainTex");
        
        public EControllerType ControllerType => EControllerType.EntityRender;
        
        private readonly IEntityManager _entityManager;
        private readonly SpriteMeshSystemConfig _config;
        private readonly ISpriteColorSystem _colorSystem;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _query;
        private NativeArrayUtil _arrayUtil;
        private MeshRenderingUtil _meshUtil;
        private VertexAttributeDescriptor[] _meshLayout;
        private List<Mesh> _meshes;
        private List<Mesh> _meshesForMeshArray;
        private Matrix4x4 _matrixDefault;
        private MeshUpdateFlags _meshUpdateFlags;
        private Material _material;
        private ProfilingHandle _profiler;

        public SpriteMeshSystem(IEntityManager entityManager, SpriteMeshSystemConfig config,
            ISpriteColorSystem colorSystem, IProfilingManager profilingManager)
        {
            _entityManager = entityManager;
            _config = config;
            _colorSystem = colorSystem;
            _profilingManager = profilingManager;
        }
        
        public void Initialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _material = new Material(_config.Shader);
            _matrixDefault = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            _meshes = new List<Mesh>();
            _meshesForMeshArray = new List<Mesh>();
            _meshUpdateFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;
            _meshLayout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2)
            };
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(SpriteRenderComponent),
                typeof(SizeComponent)
            });
        }

        public void Update()
        {
            _profiler.BeginSample("Query chunks");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query chunks");
            
            _profiler.BeginSample("Compute offsets");
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
            _profiler.EndSample("Compute offsets");

            _profiler.BeginSample("Compute meshes");
            var meshDataArray = Mesh.AllocateWritableMeshData(meshCount);
            var computeJobHandles = _arrayUtil.CreateTempJobArray<JobHandle>(meshCount);
            var positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true);
            var spriteHandle = _entityManager.GetComponentTypeHandle<SpriteRenderComponent>(true);
            var rotationHandle = _entityManager.GetComponentTypeHandle<RotationComponent>(true);
            var sizeHandle = _entityManager.GetComponentTypeHandle<SizeComponent>(true);
            var atlasChunks = _colorSystem.Chunks;
            var atlasSize = new int2(_colorSystem.Texture.width, _colorSystem.Texture.height);
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
                    spriteHandle = spriteHandle,
                    rotationHandle = rotationHandle,
                    sizeHandle = sizeHandle,
                    inChunkCount = meshChunkCount,
                    inFirstChunkIndex = chunkOffset,
                    inAtlasChunks = atlasChunks,
                    inAtlasSize = atlasSize,
                    outIndices = meshData.GetIndexData<ushort>(),
                    outVertices = meshData.GetVertexData<SpriteVertexData>()
                };
                computeJobHandles[i] = job.Schedule();

                chunkOffset += meshChunkCount;
            }

            var computeHandle = JobHandle.CombineDependencies(computeJobHandles);
            computeHandle.Complete();
            _profiler.EndSample("Compute meshes");

            _profiler.BeginSample("Create meshes");
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
            _profiler.EndSample("Create meshes");
            
            _profiler.BeginSample("Apply and dispose writable mesh data");
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _meshesForMeshArray, _meshUpdateFlags);
            _profiler.EndSample("Apply and dispose writable mesh data");
            
            _profiler.BeginSample("Draw mesh");
            _material.SetTexture(MainTexturePropertyId, _colorSystem.Texture);
            for (var i = 0; i < meshCount; i++)
            {
                _meshUtil.DrawMesh(new MeshDrawingData
                {
                    mesh = _meshes[i],
                    material = _material,
                    matrix = _matrixDefault
                });
            }
            _profiler.EndSample("Draw mesh");
            
            _profiler.BeginSample("Dispose native arrays");
            chunks.Dispose();
            chunkPerMesh.Dispose();
            spritePerMesh.Dispose();
            computeJobHandles.Dispose();
            _profiler.EndSample("Dispose native arrays");
            
            SpaceDebug.LogState("SpriteCount", totalSpriteCount);
            SpaceDebug.LogState("SpriteMeshCount", meshCount);
        }

        public void FinalizeObject()
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