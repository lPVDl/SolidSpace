using System.Collections.Generic;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Playground.Tools.ImageSpawn;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Splitting
{
    public class SplittingCommandSystem : ISplittingCommandSystem, IInitializable, IUpdatable
    {
        private readonly IEntityManager _entityManager;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly ISpriteColorSystem _spriteSystem;

        private HashSet<Entity> _checkingQueue;
        private JobMemoryAllocator _jobMemory;

        public SplittingCommandSystem(IEntityManager entityManager, IHealthAtlasSystem healthSystem, 
            ISpriteColorSystem spriteSystem)
        {
            _entityManager = entityManager;
            _healthSystem = healthSystem;
            _spriteSystem = spriteSystem;
        }
        
        public void OnInitialize()
        {
            _checkingQueue = new HashSet<Entity>();
            _jobMemory = new JobMemoryAllocator();
        }
        
        public void ScheduleSplittingCheck(Entity entity)
        {
            _checkingQueue.Add(entity);
        }

        public void OnUpdate()
        {
            foreach (var entity in _checkingQueue)
            {
                TrySplitEntity(entity);
            }

            _jobMemory.DisposeAllocations();
            _checkingQueue.Clear();
        }

        private void TrySplitEntity(Entity parentEntity)
        {
            var parentSize = _entityManager.GetComponentData<RectSizeComponent>(parentEntity).value;
            var parentSizeInt = new int2((int) parentSize.x, (int) parentSize.y);
            var parentHealthIndex = _entityManager.GetComponentData<HealthComponent>(parentEntity).index;
            var parentHealthOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks[parentHealthIndex.chunkId], parentHealthIndex);
            var parentFrameLength = HealthFrameBitsUtil.GetRequiredByteCount(parentSizeInt.x, parentSizeInt.y);

            var seedJob = new ShapeSeedJob
            {
                inFrameBits = new NativeSlice<byte>(_healthSystem.Data, parentHealthOffset, parentFrameLength),
                inFrameSize = parentSizeInt,
                outConnections = _jobMemory.CreateNativeArray<byte2>(256),
                outConnectionCount = _jobMemory.CreateNativeReference<int>(),
                outResultCode = _jobMemory.CreateNativeReference<EShapeSeedResult>(),
                outSeedBounds = _jobMemory.CreateNativeArray<ByteBounds>(256),
                outSeedCount = _jobMemory.CreateNativeReference<int>(),
                outSeedMask = _jobMemory.CreateNativeArray<byte>(parentSizeInt.x * parentSizeInt.y)
            };
            seedJob.Schedule().Complete();

            var readJob = new ShapeReadJob
            {
                inConnections = seedJob.outConnections,
                inConnectionCount = seedJob.outConnectionCount.Value,
                inOutBounds = seedJob.outSeedBounds,
                inSeedCount = seedJob.outSeedCount.Value,
                outShapeCount = _jobMemory.CreateNativeReference<int>(),
                outShapeRootSeeds = _jobMemory.CreateNativeArray<byte>(256)
            };
            readJob.Schedule().Complete();
            
            var resultCode = seedJob.outResultCode.Value;
            if (resultCode != EShapeSeedResult.Normal)
            {
                Debug.LogError($"Seed job ended with code '{resultCode}'");
                return;
            }

            var childCount = readJob.outShapeCount.Value;
            if (childCount <= 1)
            {
                return;
            }
            
            var handleCount = 0;
            var handles = _jobMemory.CreateNativeArray<JobHandle>(childCount * 2);
            var parentPosition = _entityManager.GetComponentData<PositionComponent>(parentEntity).value;
            var parentRotation = _entityManager.GetComponentData<RotationComponent>(parentEntity).value;
            var parentSprite = _entityManager.GetComponentData<SpriteRenderComponent>(parentEntity).index;
            var parentSpriteOffset = AtlasMath.ComputeOffset(_spriteSystem.Chunks[parentSprite.chunkId], parentSprite);
            var spriteSystemTexture = _spriteSystem.Texture;
            var spriteSystemTextureSize = new int2(spriteSystemTexture.width, spriteSystemTexture.height);
            var spriteSystemTexturePtr = _spriteSystem.Texture.GetRawTextureData<ColorRGB24>();
            
            for (var i = 0; i < childCount; i++)
            {
                var childBounds = readJob.inOutBounds[i];
                var childWidth = childBounds.max.x - childBounds.min.x + 1;
                var childHeight = childBounds.max.y - childBounds.min.y + 1;
                if (childWidth > 32 || childHeight > 32)
                {
                    continue;
                }

                var childEntity = _entityManager.Instantiate(parentEntity);
                _entityManager.SetComponentData(childEntity, new RectSizeComponent
                {
                    value = new half2((half) childWidth, (half) childHeight)
                });
                
                var childLocalPosX = (childBounds.max.x + childBounds.min.x + 1) * 0.5f - parentSize.x * 0.5f;
                var childLocalPosY = (childBounds.max.y + childBounds.min.y + 1) * 0.5f - parentSize.y * 0.5f;
                FloatMath.SinCos(parentRotation, out var childSin, out var childCos);
                _entityManager.SetComponentData(childEntity, new PositionComponent
                {
                    value = FloatMath.Rotate(childLocalPosX, childLocalPosY, childSin, childCos) + parentPosition
                });
                
                _entityManager.SetComponentData(childEntity, new RotationComponent
                {
                    value = parentRotation
                });
                
                var childSprite = _spriteSystem.Allocate(childWidth, childHeight);
                _entityManager.SetComponentData(childEntity, new SpriteRenderComponent
                {
                    index = childSprite
                });
                var childSpriteOffset = AtlasMath.ComputeOffset(_spriteSystem.Chunks[childSprite.chunkId], childSprite);
                handles[handleCount++] = new BlitShapeLinear24Job
                {
                    inConnections = seedJob.outConnections,
                    inConnectionCount = seedJob.outConnectionCount.Value,
                    inSourceTextureOffset = parentSpriteOffset + new int2(childBounds.min.x, childBounds.min.y),
                    inSourceTextureSize = spriteSystemTextureSize,
                    inSourceSeedMaskOffset = new int2(childBounds.min.x, childBounds.min.y),
                    inSourceSeedMaskSize = parentSizeInt,
                    inBlitSize = new int2(childWidth, childHeight),
                    inSourceTexture = spriteSystemTexturePtr,
                    inTargetOffset = childSpriteOffset,
                    inTargetSize = spriteSystemTextureSize,
                    inBlitShapeSeed = readJob.outShapeRootSeeds[i],
                    inSourceSeedMask = seedJob.outSeedMask,
                    outTargetTexture = spriteSystemTexturePtr
                }.Schedule();
                
                var childHealth = _healthSystem.Allocate(childWidth, childHeight);
                _entityManager.SetComponentData(childEntity, new HealthComponent
                {
                    index = childHealth
                });
                var childHealthOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks[childHealth.chunkId], childHealth);
                handles[handleCount++] = new BuildShapeHealthJob
                {
                    inConnections = seedJob.outConnections,
                    inConnectionCount = seedJob.outConnectionCount.Value,
                    inSourceOffset = new int2(childBounds.min.x, childBounds.min.y),
                    inBlitSize = new int2(childWidth, childHeight),
                    inSourceSize = parentSizeInt,
                    inTargetOffset = childHealthOffset,
                    inBlitShapeSeed = readJob.outShapeRootSeeds[i],
                    inSourceSeedMask = seedJob.outSeedMask,
                    outTargetHealth = _healthSystem.Data
                    
                }.Schedule();
            }
            
            _entityManager.DestroyEntity(parentEntity);
            
            JobHandle.CombineDependencies(new NativeSlice<JobHandle>(handles, 0, handleCount)).Complete();
        }

        public void OnFinalize()
        {
            
        }
    }
}