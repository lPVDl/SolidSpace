using System;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.Mathematics;
using SolidSpace.Playground.Tools.ImageSpawn;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Splitting
{
    public class SplittingController
    {
        private readonly IEntityManager _entityManager;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly ISpriteColorSystem _spriteSystem;

        public SplittingController(IEntityManager entityManager, IHealthAtlasSystem healthSystem, 
            ISpriteColorSystem spriteSystem)
        {
            _entityManager = entityManager;
            _healthSystem = healthSystem;
            _spriteSystem = spriteSystem;
        }
        
        public SplittingContext UpdateState(SplittingContext context)
        {
            return context.state switch
            {
                ESplittingState.NotStarted => OnNotStarted(context),
                ESplittingState.SeedJob => OnSeedJob(context),
                ESplittingState.ReadJob => OnReadJob(context),
                ESplittingState.BlitJob => OnBlitJob(context),
                
                _ => throw new ArgumentOutOfRangeException($"Could not process state '{context.state}'")
            };
        }

        private SplittingContext OnNotStarted(SplittingContext context)
        {
            var entitySize = _entityManager.GetComponentData<RectSizeComponent>(context.entity).value;
            var entitySizeInt = new int2((int) entitySize.x, (int) entitySize.y);
            var healthIndex = _entityManager.GetComponentData<HealthComponent>(context.entity).index;
            var healthOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks[healthIndex.chunkId], healthIndex);
            var frameLength = HealthFrameBitsUtil.GetRequiredByteCount(entitySizeInt.x, entitySizeInt.y);

            context.seedJob = new ShapeSeedJob
            {
                inFrameBits = new NativeSlice<byte>(_healthSystem.Data, healthOffset, frameLength),
                inFrameSize = entitySizeInt,
                outConnections = context.jobMemory.CreateNativeArray<byte2>(256),
                outConnectionCount = context.jobMemory.CreateNativeReference<int>(),
                outResultCode = context.jobMemory.CreateNativeReference<EShapeSeedResult>(),
                outSeedBounds = context.jobMemory.CreateNativeArray<ByteBounds>(256),
                outSeedCount = context.jobMemory.CreateNativeReference<int>(),
                outSeedMask = context.jobMemory.CreateNativeArray<byte>(entitySizeInt.x * entitySizeInt.y)
            };
            context.jobHandle = context.seedJob.Schedule();
            context.state = ESplittingState.SeedJob;
            context.jobDifficulty = entitySizeInt.x * entitySizeInt.y;

            return context;
        }

        private SplittingContext OnSeedJob(SplittingContext context)
        {
            if (!context.jobHandle.IsCompleted)
            {
                return context;
            }

            context.jobHandle.Complete();
            
            var resultCode = context.seedJob.outResultCode.Value;
            if (resultCode != EShapeSeedResult.Normal)
            {
                Debug.LogError($"Seed job ended with code '{resultCode}'");
                context.state = ESplittingState.Completed;
                return context;
            }

            context.readJob = new ShapeReadJob
            {
                inOutConnections = context.seedJob.outConnections,
                inConnectionCount = context.seedJob.outConnectionCount.Value,
                inOutBounds = context.seedJob.outSeedBounds,
                inSeedCount = context.seedJob.outSeedCount.Value,
                outShapeCount = context.jobMemory.CreateNativeReference<int>(),
                outShapeRootSeeds = context.jobMemory.CreateNativeArray<byte>(256)
            };
            context.jobHandle = context.readJob.Schedule();
            context.state = ESplittingState.ReadJob;
            context.jobDifficulty = 0;
            
            return context;
        }

        private SplittingContext OnReadJob(SplittingContext context)
        {
            if (!context.jobHandle.IsCompleted)
            {
                return context;
            }
            
            context.jobHandle.Complete();

            var childCount = context.readJob.outShapeCount.Value;
            if (childCount == 0)
            {
                context.state = ESplittingState.Completed;
                _entityManager.DestroyEntity(context.entity);
                return context;
            }
            
            var parentSize = _entityManager.GetComponentData<RectSizeComponent>(context.entity).value;
            var parentSizeInt = new int2((int) parentSize.x, (int) parentSize.y);
            if (childCount == 1)
            {
                var childBounds = context.readJob.inOutBounds[0];
                var childWidth = childBounds.max.x - childBounds.min.x + 1;
                var childHeight = childBounds.max.y - childBounds.min.y + 1;
                if ((childWidth == parentSizeInt.x) && (childHeight == parentSizeInt.y))
                {
                    context.state = ESplittingState.Completed;
                    return context;
                }
            }
            
            var handleCount = 0;
            var handles = context.jobMemory.CreateNativeArray<JobHandle>(childCount * 2);
            var parentPosition = _entityManager.GetComponentData<PositionComponent>(context.entity).value;
            var parentRotation = _entityManager.GetComponentData<RotationComponent>(context.entity).value;
            var parentSprite = _entityManager.GetComponentData<SpriteRenderComponent>(context.entity).index;
            var parentSpriteOffset = AtlasMath.ComputeOffset(_spriteSystem.Chunks[parentSprite.chunkId], parentSprite);
            var spriteSystemTexture = _spriteSystem.Texture;
            var spriteSystemTextureSize = new int2(spriteSystemTexture.width, spriteSystemTexture.height);
            var spriteSystemTextureData = _spriteSystem.Texture.GetRawTextureData<ColorRGB24>();

            for (var i = 0; i < childCount; i++)
            {
                var childBounds = context.readJob.inOutBounds[i];
                var childWidth = childBounds.max.x - childBounds.min.x + 1;
                var childHeight = childBounds.max.y - childBounds.min.y + 1;
                if (childWidth > 32 || childHeight > 32)
                {
                    continue;
                }
                
                var childEntity = _entityManager.Instantiate(context.entity);
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
                    inConnections = context.seedJob.outConnections,
                    inConnectionCount = context.seedJob.outConnectionCount.Value,
                    inSourceTextureOffset = parentSpriteOffset + new int2(childBounds.min.x, childBounds.min.y),
                    inSourceTextureSize = spriteSystemTextureSize,
                    inSourceSeedMaskOffset = new int2(childBounds.min.x, childBounds.min.y),
                    inSourceSeedMaskSize = parentSizeInt,
                    inBlitSize = new int2(childWidth, childHeight),
                    inSourceTexture = spriteSystemTextureData,
                    inTargetOffset = childSpriteOffset,
                    inTargetSize = spriteSystemTextureSize,
                    inBlitShapeSeed = context.readJob.outShapeRootSeeds[i],
                    inSourceSeedMask = context.seedJob.outSeedMask,
                    outTargetTexture = spriteSystemTextureData
                }.Schedule();
                
                var childHealth = _healthSystem.Allocate(childWidth, childHeight);
                _entityManager.SetComponentData(childEntity, new HealthComponent
                {
                    index = childHealth
                });
                var childHealthOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks[childHealth.chunkId], childHealth);
                handles[handleCount++] = new BuildShapeHealthJob
                {
                    inConnections = context.seedJob.outConnections,
                    inConnectionCount = context.seedJob.outConnectionCount.Value,
                    inSourceOffset = new int2(childBounds.min.x, childBounds.min.y),
                    inBlitSize = new int2(childWidth, childHeight),
                    inSourceSize = parentSizeInt,
                    inTargetOffset = childHealthOffset,
                    inBlitShapeSeed = context.readJob.outShapeRootSeeds[i],
                    inSourceSeedMask = context.seedJob.outSeedMask,
                    outTargetHealth = _healthSystem.Data
                }.Schedule();
            }
            
            _entityManager.DestroyEntity(context.entity);

            context.jobHandle = JobHandle.CombineDependencies(new NativeSlice<JobHandle>(handles, 0, handleCount));
            context.state = ESplittingState.BlitJob;
            context.jobDifficulty = int.MaxValue;

            return context;
        }

        private SplittingContext OnBlitJob(SplittingContext context)
        {
            if (!context.jobHandle.IsCompleted)
            {
                return context;
            }

            context.jobHandle.Complete();
            
            context.state = ESplittingState.Completed;
            
            return context;
        }
    }
}