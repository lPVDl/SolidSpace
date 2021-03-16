using Unity.Entities;

namespace SpaceSimulator.Runtime.Entities.RepeatTimer
{
    public struct RepeatTimerComponent : IComponentData
    {
        public float delay;
        public float timer;
        public byte counter;
    }
}