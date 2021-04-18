using Unity.Entities;

namespace SpaceSimulator.Entities.RepeatTimer
{
    public struct RepeatTimerComponent : IComponentData
    {
        public float delay;
        public float timer;
        public byte counter;
    }
}