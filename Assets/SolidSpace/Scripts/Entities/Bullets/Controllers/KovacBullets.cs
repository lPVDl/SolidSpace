using SolidSpace.Entities.Components;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.Physics.Velcast;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Profiling;
using Unity.Entities;
using UnityEngine;

namespace SolidSpace.Entities.Bullets
{
    public class KovacBullets : IInitializable, IUpdatable
    {
        private readonly IKovacFactory _kovacFactory;
        private readonly IKovacRaycasterFactory _raycasterFactory;
        private readonly IProfilingManager _profilingManager;
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _worldTime;
        private readonly ISpriteColorSystem _spriteSystem;
        private readonly IHealthAtlasSystem _healthSystem;

        private ProfilingHandle _profiler;
        private IKovacBakery<BulletColliderBakeBehaviour> _baker;
        private IKovacRaycaster<BulletRaycastBehaviour> _raycaster;

        public KovacBullets(IKovacFactory kovacFactory, IKovacRaycasterFactory raycasterFactory, 
            IProfilingManager profilingManager, IEntityManager entityManager, IEntityWorldTime worldTime,
            ISpriteColorSystem spriteSystem, IHealthAtlasSystem healthSystem)
        {
            _kovacFactory = kovacFactory;
            _raycasterFactory = raycasterFactory;
            _profilingManager = profilingManager;
            _entityManager = entityManager;
            _worldTime = worldTime;
            _spriteSystem = spriteSystem;
            _healthSystem = healthSystem;
        }
        
        public void OnInitialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _baker = _kovacFactory.Create<BulletColliderBakeBehaviour>(_profiler, new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RectSizeComponent),
                typeof(SpriteRenderComponent),
                typeof(HealthComponent)
            });
            _raycaster = _raycasterFactory.Create<BulletRaycastBehaviour>(_profiler, new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(BulletComponent)
            });
        }
        
        public void OnUpdate()
        {
            var bakeBehaviour = new BulletColliderBakeBehaviour
            {
                healthHandle = _entityManager.GetComponentTypeHandle<HealthComponent>(true),
                spriteHandle = _entityManager.GetComponentTypeHandle<SpriteRenderComponent>(true),
            };
            
            _profiler.BeginSample("Bake colliders");
            var colliders = _baker.Bake(ref bakeBehaviour);
            _profiler.EndSample("Bake colliders");

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
            _raycaster.Raycast(colliders, ref raycastBehaviour);
            _profiler.EndSample("Raycast");

            _profiler.BeginSample("Apply damage");
            var hitCount = raycastBehaviour.outCount.Value;
            var hits = raycastBehaviour.outHits;
            var healthAtlas = _healthSystem.Data;
            var spriteTexture = _spriteSystem.Texture;
            for (var i = 0; i < hitCount; i++)
            {
                var hit = hits[i];
                _entityManager.DestroyEntity(hit.bulletEntity);
                healthAtlas[hit.healthOffset] = 0;
                spriteTexture.SetPixel(hit.spriteOffset.x, hit.spriteOffset.y, Color.black);
            }
            spriteTexture.Apply();
            _profiler.EndSample("Apply damage");
            
            bakeBehaviour.Dispose();
            raycastBehaviour.Dispose();
            colliders.Dispose();
        }

        public void OnFinalize()
        {
            
        }
    }
}