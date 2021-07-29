using SolidSpace.Entities.Components;
using SolidSpace.Entities.ParentHandle;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.ParentTransform
{
    public class ParentTransformSystem : IInitializable, IUpdatable
    {
        private readonly IEntityManager _entityManager;
        private readonly IProfilingManager _profilingManager;
        private readonly IParentHandleManager _parentHandleManager;

        private JobMemoryAllocator _jobMemory;
        private ProfilingHandle _profiler;
        private EntityQuery _parentQuery;
        private EntityQuery _childQuery;
        
        public ParentTransformSystem(IEntityManager entityManager, IProfilingManager profilingManager, 
            IParentHandleManager parentHandleManager)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
            _parentHandleManager = parentHandleManager;
        }
        
        public void OnInitialize()
        {
            _jobMemory = new JobMemoryAllocator();
            _profiler = _profilingManager.GetHandle(this);
            _parentQuery = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(ParentComponent),
                typeof(PositionComponent),
            });
            _childQuery = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(ChildComponent),
                typeof(PositionComponent),
            });
        }
        
        public void OnUpdate()
        {
            _profiler.BeginSample("Parent Collect");
            var parentCollectJob = new CollectParentTransformJob
            {
                inChunks = _jobMemory.CreateArchetypeChunkArrayFromQuery(_parentQuery),
                parentHandle = _entityManager.GetComponentTypeHandle<ParentComponent>(true),
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                rotationHandle = _entityManager.GetComponentTypeHandle<RotationComponent>(true),
                outParentData = _jobMemory.CreateNativeArray<ParentData>(_parentHandleManager.Handles.Length)
            };
            parentCollectJob.Schedule(parentCollectJob.inChunks.Length, 4).Complete();
            _profiler.EndSample("Parent Collect");

            _profiler.BeginSample("Transform Job");
            var transformJob = new ChildTransformApplyJob
            {
                inChildChunks = _jobMemory.CreateArchetypeChunkArrayFromQuery(_childQuery),
                inParentData = parentCollectJob.outParentData,
                childHandle = _entityManager.GetComponentTypeHandle<ChildComponent>(true),
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(false),
                rotationHandle = _entityManager.GetComponentTypeHandle<RotationComponent>(false),
                localPositionHandle = _entityManager.GetComponentTypeHandle<LocalPositionComponent>(true),
            };
            transformJob.Schedule(transformJob.inChildChunks.Length, 2).Complete();
            _profiler.EndSample("Transform Job");
            
            _jobMemory.DisposeAllocations();
        }

        public void OnFinalize()
        {
            
        }
    }
}