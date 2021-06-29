using SolidSpace.Entities.World;
using SolidSpace.GameCycle;

namespace SolidSpace.Entities.Bullets
{
    internal class BulletCommandSystem : IUpdatable
    {
        private readonly IBulletComputeSystem _computeSystem;
        private readonly IEntityManager _entityManager;

        public BulletCommandSystem(IBulletComputeSystem computeSystem, IEntityManager entityManager)
        {
            _computeSystem = computeSystem;
            _entityManager = entityManager;
        }
        
        public void OnUpdate()
        {
            _entityManager.DestroyEntity(_computeSystem.EntitiesToDestroy);
        }
    }
}