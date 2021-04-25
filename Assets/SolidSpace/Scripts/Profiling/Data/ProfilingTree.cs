using Unity.Collections;

namespace SolidSpace.Profiling.Data
{
    public struct ProfilingTree
    {
        public string[] text;
        public NativeArray<ushort> childs;
        public NativeArray<ushort> siblings;
        public NativeArray<ushort> names;
    }
}