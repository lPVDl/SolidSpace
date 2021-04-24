using Unity.Entities;

namespace SolidSpace.Entities.RepeatTimer
{
    public struct RepeatTimerComponent : IComponentData
    {
        public float delay;
        public float timer;
        public byte counter;
    }
}