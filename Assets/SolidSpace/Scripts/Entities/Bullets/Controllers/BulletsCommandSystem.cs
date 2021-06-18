using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.Physics.Raycast;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace SolidSpace.Entities.Bullets
{
    internal class BulletsCommandSystem : IInitializable, IUpdatable
    {
        private readonly ISpriteColorSystem _colorSystem;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly IColliderSystem _colliderSystem;
        private readonly IRaycastSystem _raycastSystem;
        private readonly IProfilingManager _profilingManager;
        private readonly IEntityWorldManager _entityManager;

        private ProfilingHandle _profiler;
        private NativeHashSet<ComponentType> _shipComponents;
        private NativeHashSet<ComponentType> _bulletComponents;

        public BulletsCommandSystem(ISpriteColorSystem colorSystem, IHealthAtlasSystem healthSystem,
            IColliderSystem colliderSystem, IRaycastSystem raycastSystem, IProfilingManager profilingManager,
            IEntityWorldManager entityManager)
        {
            _colorSystem = colorSystem;
            _healthSystem = healthSystem;
            _colliderSystem = colliderSystem;
            _raycastSystem = raycastSystem;
            _profilingManager = profilingManager;
            _entityManager = entityManager;
        }
        
        public void Initialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _shipComponents = new NativeHashSet<ComponentType>(2, Allocator.Persistent)
            {
                typeof(HealthComponent),
                typeof(SpriteComponent)
            };
            _bulletComponents = new NativeHashSet<ComponentType>(3, Allocator.Persistent)
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(BulletComponent),
            };
        }

        public void Update()
        {
            var raycastWorld = _raycastSystem.World;
            var colliderWorld = _colliderSystem.World;
            
            _profiler.BeginSample("Create Filter");
            var colliderFilter = FilterArchetypes(colliderWorld.archetypes, _shipComponents);
            var raycasterFilter = FilterArchetypes(raycastWorld.raycastArchetypes, _bulletComponents);
            _profiler.EndSample("Create Filter");
            
            _profiler.BeginSample("Execute Filter");
            var hitCount = raycastWorld.raycastEntities.Length;
            var jobCount = (int) Math.Ceiling(hitCount / 128f);
            var filterCounts = NativeMemory.CreateTempJobArray<int>(jobCount);
            var filterIndices = NativeMemory.CreateTempJobArray<int>(hitCount);
            new RaycastWorldFilterJob
            {
                inTotalItem = hitCount,
                inItemPerJob = 128,
                inColliderIndices = raycastWorld.colliderIndices,
                inColliderArchetypeIndices = colliderWorld.colliderArchetypeIndices,
                inColliderArchetypesFilter = colliderFilter,
                inRaycasterArchetypeFilter = raycasterFilter,
                inRaycasterArchetypeIndices = raycastWorld.raycastArchetypeIndices,
                outCounts = filterCounts,
                outIndices = filterIndices
            }.Schedule(jobCount, 4).Complete();
            _profiler.EndSample("Execute Filter");

            _profiler.BeginSample("Collect Filter");
            var arrayCount = NativeMemory.CreateTempJobReference<int>();
            new DataCollectJob<int>
            {
                inCounts = filterCounts,
                inOutData = filterIndices,
                inOffset = 128,
                outCount = arrayCount
            }.Schedule().Complete();
            _profiler.EndSample("Collect Filter");
            
            _profiler.BeginSample("Query Collider Data");
            var colliderEntities = colliderWorld.colliderEntities;
            var colliderArchetypeIndices = colliderWorld.colliderArchetypeIndices;
            var colliderCount = colliderEntities.Length;
            var colliderHealth = NativeMemory.CreateTempJobArray<HealthComponent>(colliderCount);
            var colliderSprites = NativeMemory.CreateTempJobArray<SpriteComponent>(colliderCount);
            for (var i = 0; i < colliderCount; i++)
            {
                var colliderEntity = colliderEntities[i];
                var colliderArchetype = colliderArchetypeIndices[i];
                if (!colliderFilter.Contains(colliderArchetype))
                {
                    continue;
                }
                
                colliderHealth[i] = _entityManager.GetComponentData<HealthComponent>(colliderEntity);
                colliderSprites[i] = _entityManager.GetComponentData<SpriteComponent>(colliderEntity);
            }
            _profiler.EndSample("Query Collider Data");
            
            _profiler.BeginSample("Raycast Compute");
            var estimatedHitCount = arrayCount.Value;
            jobCount = (int) Math.Ceiling(estimatedHitCount / 16f);
            var bulletCastCounts = NativeMemory.CreateTempJobArray<int>(jobCount);
            var raycastResult = NativeMemory.CreateTempJobArray<BulletHit>(estimatedHitCount);
            new BulletCastJob
            {
                inItemPerJob = 16,
                inItemTotal = estimatedHitCount,
                inHealthAtlas = _healthSystem.Data,
                inHealthChunks = _healthSystem.Chunks,
                inSpriteChunks = _colorSystem.Chunks,
                inSpriteComponents = colliderSprites,
                inColliderWorld = colliderWorld,
                inRaycastWorld = raycastWorld,
                inHealthComponents = colliderHealth,
                inFilteredIndices = filterIndices,
                outCounts = bulletCastCounts,
                outHits = raycastResult,
            }.Schedule(jobCount, 1).Complete();
            _profiler.EndSample("Raycast Compute");
            
            _profiler.BeginSample("Raycast Collect");
            new DataCollectJob<BulletHit>
            {
                inCounts = bulletCastCounts,
                inOffset = 16,
                inOutData = raycastResult,
                outCount = arrayCount
            }.Schedule().Complete();
            _profiler.EndSample("Raycast Collect");
            
            _profiler.BeginSample("Apply Damage");
            var bulletCount = arrayCount.Value;
            var healthAtlas = _healthSystem.Data;
            var spriteTexture = _colorSystem.Texture;
            for (var i = 0; i < bulletCount; i++)
            {
                var rayHit = raycastResult[i];
                _entityManager.DestroyEntity(rayHit.bulletEntity);
                healthAtlas[rayHit.healthOffset] = 0;
                spriteTexture.SetPixel(rayHit.spriteOffset.x, rayHit.spriteOffset.y, Color.black);
            }
            
            spriteTexture.Apply();
            _profiler.EndSample("Apply Damage");

            _profiler.BeginSample("Dispose Arrays");
            colliderFilter.Dispose();
            raycasterFilter.Dispose();
            filterCounts.Dispose();
            filterIndices.Dispose();
            bulletCastCounts.Dispose();
            arrayCount.Dispose();
            colliderHealth.Dispose();
            colliderSprites.Dispose();
            raycastResult.Dispose();
            _profiler.EndSample("Dispose Arrays");
        }

        private NativeHashSet<byte> FilterArchetypes(NativeSlice<EntityArchetype> archetypes,
            NativeHashSet<ComponentType> requiredComponents)
        {
            var outArchetypes = new NativeHashSet<byte>(archetypes.Length, Allocator.TempJob);
            var requireCount = requiredComponents.Count();
            for (var i = 0; i < archetypes.Length; i++)
            {
                using var components = archetypes[i].GetComponentTypes();
                
                var count = 0;
                for (var j = 0; j < components.Length; j++)
                {
                    if (requiredComponents.Contains(components[j]))
                    {
                        count++;
                    }
                }

                if (count == requireCount)
                {
                    outArchetypes.Add((byte) i);
                }
            }

            return outArchetypes;
        }

        public void Finalize()
        {
            _shipComponents.Dispose();
            _bulletComponents.Dispose();
        }
    }
}