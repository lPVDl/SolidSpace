using SolidSpace.Entities.Parenting;
using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct ChildComponent : IComponentData
    {
        private ParentHandleInfo parentHandle;
    }
}