using Unity.Collections;

namespace SolidSpace.Entities.Parenting
{
    public interface IParentHandleManager
    {
        public NativeSlice<HandleState> Handles { get; }

        ParentHandleInfo AllocateHandle();

        void ReleaseHandle(ushort handleIndex);
    }
}