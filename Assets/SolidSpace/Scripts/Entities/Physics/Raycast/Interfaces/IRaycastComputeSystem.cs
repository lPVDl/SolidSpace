using Unity.Collections;

namespace SolidSpace.Entities.Physics.Raycast
{
    internal interface IRaycastComputeSystem
    {
        RaycastWorldData RaycastWorld { get; }
    }
}