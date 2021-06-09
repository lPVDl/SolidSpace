using Unity.Mathematics;

namespace SolidSpace.Playground.Sandbox.Core
{
    public interface IPointerTracker
    {
        public float2 Position { get; }

        public bool ClickedThisFrame { get; }
    }
}