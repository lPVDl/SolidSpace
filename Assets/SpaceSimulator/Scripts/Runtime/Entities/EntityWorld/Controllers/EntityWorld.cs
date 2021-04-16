using Unity.Core;
using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities
{
    public class EntityWorld : IEntitySystem, IEntityWorld
    {
        public ESystemType SystemType => ESystemType.World;

        public EntityManager EntityManager => _world.EntityManager;

        public TimeData Time { get; private set; }

        private readonly World _world;
        
        public EntityWorld()
        {
            _world = new World("SpaceSimulator");
        }
        
        public void Initialize()
        {
            
        }

        public void Update()
        {
            var deltaTime = UnityEngine.Time.deltaTime;
            Time = new TimeData(Time.ElapsedTime + deltaTime, deltaTime);
        }

        public void FinalizeSystem()
        {
            
        }
    }
}