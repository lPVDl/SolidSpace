using SolidSpace.Entities.Actors.Interfaces;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Gizmos;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Actors
{
    public class ActorControlSystem : IInitializable, IUpdatable, IActorControlSystem
    {
        private readonly IEntityWorldManager _entityManager;
        private readonly IProfilingManager _profilingManager;
        private readonly IEntityWorldTime _worldTime;
        private readonly IGizmosManager _gizmosManager;

        private EntityQuery _query;
        private float2 _targetPosition;
        private ProfilingHandle _profiler;
        private GizmosHandle _gizmos;

        public ActorControlSystem(IEntityWorldManager entityManager, IProfilingManager profilingManager, 
            IEntityWorldTime worldTime, IGizmosManager gizmosManager)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
            _worldTime = worldTime;
            _gizmosManager = gizmosManager;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this, Color.magenta);
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(RotationComponent),
                typeof(ActorComponent)
            });
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Query chunks");
            var archetypeChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query chunks");
            
            _profiler.BeginSample("Control Job");
            new ActorControlJob
            {
                inArchetypeChunks = archetypeChunks,
                inDeltaTime = _worldTime.DeltaTime,
                inSeekPosition = _targetPosition,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                velocityHandle = _entityManager.GetComponentTypeHandle<VelocityComponent>(false),
                actorHandle = _entityManager.GetComponentTypeHandle<ActorComponent>(true),
                rotationHandle = _entityManager.GetComponentTypeHandle<RotationComponent>(false)
                
            }.Schedule(archetypeChunks.Length, 4).Complete();
            _profiler.EndSample("Control Job");
            
            _profiler.BeginSample("Gizmos offsets");
            var chunkCount = archetypeChunks.Length;
            var offsets = NativeMemory.CreateTempJobArray<int>(chunkCount);
            var counts = NativeMemory.CreateTempJobArray<int>(chunkCount);
            var maxEntityCount = 0;
            for (var i = 0; i < chunkCount; i++)
            {
                offsets[i] = maxEntityCount;
                maxEntityCount += archetypeChunks[i].Count;
            }
            var positions = NativeMemory.CreateTempJobArray<float2>(maxEntityCount);
            _profiler.EndSample("Gizmos offsets");
            
            _profiler.BeginSample("Gizmos filter");
            new FilterActivateActorsJob
            {
                inArchetypeChunks = archetypeChunks,
                inWriteOffsets = offsets,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                actorHandle = _entityManager.GetComponentTypeHandle<ActorComponent>(true),
                outPositions = positions,
                outCounts = counts
            }.Schedule(chunkCount, 4).Complete();
            _profiler.EndSample("Gizmos filter");
            
            _profiler.BeginSample("Gizmos collect result");
            var countReference = NativeMemory.CreateTempJobReference<int>();
            new DataCollectJobWithOffsets<float2>
            {
                inCounts = counts,
                inOffsets = offsets,
                inOutData = positions,
                outCount = countReference
            }.Schedule().Complete();
            _profiler.EndSample("Gizmos collect result");
            
            _profiler.BeginSample("Draw gizmos");
            _gizmos.DrawWirePolygon(_targetPosition, 100f, 48);
            var count = countReference.Value;
            for (var i = 0; i < count; i++)
            {
                _gizmos.DrawLine(positions[i], _targetPosition);
            }
            _profiler.EndSample("Draw gizmos");

            _profiler.BeginSample("Dispose arrays");
            archetypeChunks.Dispose();
            offsets.Dispose();
            counts.Dispose();
            positions.Dispose();
            countReference.Dispose();
            _profiler.EndSample("Dispose arrays");
        }

        public void SetActorsTargetPosition(float2 position)
        {
            _targetPosition = position;
        }
        
        public void OnFinalize()
        {
            
        }
    }
}