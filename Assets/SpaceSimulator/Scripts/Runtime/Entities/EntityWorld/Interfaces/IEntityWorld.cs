using Unity.Core;
using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities
{
    public interface IEntityWorld
    {
        public EntityManager EntityManager { get; }
    }
}