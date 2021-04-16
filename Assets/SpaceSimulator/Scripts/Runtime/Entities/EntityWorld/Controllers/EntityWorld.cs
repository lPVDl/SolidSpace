using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities
{
    public class EntityWorld : IEntityWorld
    {
        public EntityManager EntityManager => _world.EntityManager;

        private readonly World _world;
        
        public EntityWorld()
        {
            _world = new World("SpaceSimulator");
        }
    }
}