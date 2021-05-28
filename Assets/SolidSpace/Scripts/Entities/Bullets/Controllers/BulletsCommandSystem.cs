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

namespace SolidSpace.Entities.Bullets
{
    internal class BulletsCommandSystem : IController
    {
        public EControllerType ControllerType => EControllerType.EntityCommand;
        
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
        
        public void InitializeController()
        {
            _profiler = _profilingManager.GetHandle(this);
            _shipComponents = new NativeHashSet<ComponentType>(2, Allocator.Persistent)
            {
                typeof(HealthComponent),
                typeof(SpriteRenderComponent)
            };
            _bulletComponents = new NativeHashSet<ComponentType>(3, Allocator.Persistent)
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(BulletComponent),
            };
        }

        public void UpdateController()
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
            var filterCount = NativeMemory.CreateTempJobReference<int>();
            new DataCollectJob<int>
            {
                inCounts = filterCounts,
                inOutData = filterIndices,
                inOffset = 128,
                outCount = filterCount
            }.Schedule().Complete();
            _profiler.EndSample("Collect Filter");
            
            _profiler.BeginSample("Raycaster Entity Array");
            var raycasterEntities = NativeMemory.CreateTempJobArray<Entity>(filterCount.Value);
            new PickItemsByIndexJob<Entity>
            {
                inItems = raycastWorld.raycastEntities,
                inItemIndices = filterIndices,
                inItemIndexCount = filterCount.Value,
                outItems = raycasterEntities
            }.Schedule().Complete();
            _profiler.EndSample("Raycaster Entity Array");
            
            _profiler.BeginSample("Destroy Entities");
            _entityManager.DestroyEntity(raycasterEntities);
            _profiler.EndSample("Destroy Entities");
            
            _profiler.BeginSample("Dispose Arrays");
            colliderFilter.Dispose();
            raycasterFilter.Dispose();
            filterCounts.Dispose();
            filterIndices.Dispose();
            filterCount.Dispose();
            raycasterEntities.Dispose();
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

        public void FinalizeController()
        {
            _shipComponents.Dispose();
            _bulletComponents.Dispose();
        }
    }
}