using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Playground.Entities.SearchNearestEntity
{
    internal class SearchNearestEntitySystem : IInitializable, IUpdatable, ISearchNearestEntitySystem
    {
        public EntityPosition Result { get; private set; }
        
        private readonly IEntityWorldManager _entityManager;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _query;
        private ProfilingHandle _profiler;
        private float2 _searchPosition;
        private bool _enabled;

        public SearchNearestEntitySystem(IEntityWorldManager entityManager, IProfilingManager profilingManager)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
        }
        
        public void OnInitialize()
        {
            _profiler = _profilingManager.GetHandle(this);
        }
        
        public void SetSearchPosition(float2 position)
        {
            _searchPosition = position;
        }
        
        public void SetQuery(EntityQueryDesc queryDesc)
        {
            _query = _entityManager.CreateEntityQuery(queryDesc);
        }
        
        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }
        
        public void OnUpdate()
        {
            Result = new EntityPosition
            {
                isValid = false
            };
            
            if (!_enabled)
            {
                return;
            }

            var archetypeChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            if (archetypeChunks.Length == 0)
            {
                archetypeChunks.Dispose();
                return;
            }

            _profiler.BeginSample("Job");
            var positions = NativeMemory.CreateTempJobArray<float2>(archetypeChunks.Length);
            var entities = NativeMemory.CreateTempJobArray<Entity>(archetypeChunks.Length);
            new SearchNearestEntityJob
            {
                inChunks = archetypeChunks,
                inSearchPoint = _searchPosition,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                entityHandle = _entityManager.GetEntityTypeHandle(),
                outNearestPositions = positions,
                outNearestEntities = entities
            }.Schedule(archetypeChunks.Length, 4).Complete();
            _profiler.EndSample("Job");
            
            _profiler.BeginSample("Managed");
            var minDistance = float.MaxValue;
            var minPosition = float2.zero;
            Entity minEntity = default;
            for (var i = 0; i < archetypeChunks.Length; i++)
            {
                var position = positions[i];
                var distance = FloatMath.Distance(positions[i], _searchPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minPosition = position;
                    minEntity = entities[i];
                }
            }
            _profiler.EndSample("Managed");
            
            positions.Dispose();
            entities.Dispose();
            archetypeChunks.Dispose();
            
            Result = new EntityPosition
            {
                isValid = true,
                entity = minEntity,
                position = minPosition
            };
        }

        public void OnFinalize()
        {
            
        }
    }
}