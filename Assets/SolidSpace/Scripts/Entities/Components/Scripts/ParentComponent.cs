using SolidSpace.Entities.Parenting;
using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct ParentComponent : IComponentData
    {
        public ParentHandleInfo handle;
    }
}