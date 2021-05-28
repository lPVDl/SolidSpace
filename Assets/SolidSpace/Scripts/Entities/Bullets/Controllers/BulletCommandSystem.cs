using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.Physics.Raycast;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.GameCycle;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Bullets
{
    internal class BulletCommandSystem : IController
    {
        public EControllerType ControllerType => EControllerType.EntityCommand;
        
        private readonly ISpriteColorSystem _colorSystem;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly IColliderSystem _colliderSystem;
        private readonly IRaycastSystem _raycastSystem;
        private readonly IProfilingManager _profilingManager;
        
        private ProfilingHandle _profiler;
        private NativeHashSet<ComponentType> _shipComponents;
        private NativeHashSet<ComponentType> _bulletComponents;

        public BulletCommandSystem(ISpriteColorSystem colorSystem, IHealthAtlasSystem healthSystem,
            IColliderSystem colliderSystem, IRaycastSystem raycastSystem, IProfilingManager profilingManager)
        {
            _colorSystem = colorSystem;
            _healthSystem = healthSystem;
            _colliderSystem = colliderSystem;
            _raycastSystem = raycastSystem;
            _profilingManager = profilingManager;
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
            
            _profiler.BeginSample("Create Filters");
            using var colliderFilter = FilterArchetypes(colliderWorld.archetypes, _shipComponents);
            using var raycasterFilter = FilterArchetypes(raycastWorld.raycastArchetypes, _bulletComponents);
            _profiler.EndSample("Create Filters");
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