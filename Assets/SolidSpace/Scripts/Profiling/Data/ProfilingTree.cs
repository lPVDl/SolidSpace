using Unity.Collections;

namespace SolidSpace.Profiling
{
    public struct ProfilingTree
    {
        public string[] text;
        public NativeArray<ushort> childs;
        public NativeArray<ushort> siblings;
        public NativeArray<ushort> names;
    }
}