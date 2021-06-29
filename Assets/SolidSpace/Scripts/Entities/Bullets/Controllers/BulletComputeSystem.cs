using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.Physics.Raycast;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Gizmos;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace SolidSpace.Entities.Bullets
{
    public class BulletComputeSystem : IInitializable, IUpdatable, IBulletComputeSystem
    {
        public NativeArray<Entity> EntitiesToDestroy { get; private set; }
        
        private readonly IColliderBakeSystemFactory _colliderBakeSystemFactory;
        private readonly IRaycastSystemFactory _raycastSystemFactory;
        private readonly IProfilingManager _profilingManager;
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _worldTime;
        private readonly ISpriteColorSystem _spriteSystem;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly IGizmosManager _gizmosManager;

        private ProfilingHandle _profiler;
        private IColliderBakeSystem<BulletColliderBakeBehaviour> _colliderBakeSystem;
        private IRaycastSystem<BulletRaycastBehaviour> _raycastSystem;
        private GizmosHandle _gridGizmos;
        private GizmosHandle _colliderGizmos;
        private EntityQuery _colliderQuery;
        private EntityQuery _bulletQuery;

        public BulletComputeSystem(IColliderBakeSystemFactory colliderBakeSystemFactory, IEntityWorldTime worldTime, 
            IProfilingManager profilingManager, IEntityManager entityManager, IRaycastSystemFactory raycastSystemFactory,
            ISpriteColorSystem spriteSystem, IHealthAtlasSystem healthSystem, IGizmosManager gizmosManager)
        {
            _colliderBakeSystemFactory = colliderBakeSystemFactory;
            _raycastSystemFactory = raycastSystemFactory;
            _profilingManager = profilingManager;
            _entityManager = entityManager;
            _worldTime = worldTime;
            _spriteSystem = spriteSystem;
            _healthSystem = healthSystem;
            _gizmosManager = gizmosManager;
        }
        
        public void OnInitialize()
        {
            EntitiesToDestroy = NativeMemory.CreateTempJobArray<Entity>(0);

            _gridGizmos = _gizmosManager.GetHandle(this, "Grid", Color.gray);
            _colliderGizmos = _gizmosManager.GetHandle(this, "Collider", Color.green);
            _profiler = _profilingManager.GetHandle(this);
            _colliderQuery = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RectSizeComponent),
                typeof(SpriteRenderComponent),
                typeof(HealthComponent)
            });
            _colliderBakeSystem = _colliderBakeSystemFactory.Create<BulletColliderBakeBehaviour>(_profiler);
            _bulletQuery = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(BulletComponent)
            });
            _raycastSystem = _raycastSystemFactory.Create<BulletRaycastBehaviour>(_profiler);
        }
        
        public void OnUpdate()
        {
            var bakeBehaviour = new BulletColliderBakeBehaviour
            {
                healthHandle = _entityManager.GetComponentTypeHandle<HealthComponent>(true),
                spriteHandle = _entityManager.GetComponentTypeHandle<SpriteRenderComponent>(true),
            };
            
            _profiler.BeginSample("Query colliders");
            var colliderArchetypeChunks = _colliderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query colliders");
            
            _profiler.BeginSample("Bake colliders");
            var colliders = _colliderBakeSystem.Bake(colliderArchetypeChunks, ref bakeBehaviour);
            _profiler.EndSample("Bake colliders");

            _profiler.BeginSample("Query bullets");
            var bulletArchetypeChunks = _bulletQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query bullets");
            
            var raycastBehaviour = new BulletRaycastBehaviour
            {
                inColliders = colliders,
                inColliderHealths = bakeBehaviour.outHealthComponents,
                inColliderSprites = bakeBehaviour.outSpriteComponents,
                inDeltaTime = _worldTime.DeltaTime,
                entityHandle = _entityManager.GetEntityTypeHandle(),
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                velocityHandle = _entityManager.GetComponentTypeHandle<VelocityComponent>(true),
                inHealthAtlas = _healthSystem.Data,
                inHealthChunks = _healthSystem.Chunks,
                inSpriteChunks = _spriteSystem.Chunks,
            };
            
            _profiler.BeginSample("Raycast");
            _raycastSystem.Raycast(colliders, bulletArchetypeChunks, ref raycastBehaviour);
            _profiler.EndSample("Raycast");

            _profiler.BeginSample("Apply damage");
            var hitCount = raycastBehaviour.outCount.Value;
            var hits = raycastBehaviour.outHits;
            var healthAtlas = _healthSystem.Data;
            var spriteTexture = _spriteSystem.Texture;
            
            EntitiesToDestroy.Dispose();
            var entitiesToDestroy = NativeMemory.CreateTempJobArray<Entity>(hitCount);
            EntitiesToDestroy = entitiesToDestroy;
            
            for (var i = 0; i < hitCount; i++)
            {
                var hit = hits[i];
                entitiesToDestroy[i] = hit.bulletEntity;
                healthAtlas[hit.healthOffset] = 0;
                spriteTexture.SetPixel(hit.spriteOffset.x, hit.spriteOffset.y, Color.black);
            }
            spriteTexture.Apply();
            _profiler.EndSample("Apply damage");
            
            _profiler.BeginSample("Gizmos");
            ColliderGizmosUtil.DrawGrid(_gridGizmos, colliders.grid);
            ColliderGizmosUtil.DrawColliders(_colliderGizmos, colliders);
            _profiler.EndSample("Gizmos");
            
            
            _profiler.BeginSample("Dispose arrays");
            colliderArchetypeChunks.Dispose();
            bulletArchetypeChunks.Dispose();
            bakeBehaviour.Dispose();
            raycastBehaviour.Dispose();
            colliders.Dispose();
            _profiler.EndSample("Dispose arrays");
        }

        public void OnFinalize()
        {
            EntitiesToDestroy.Dispose();
        }
    }
}