using System.Collections.Generic;
using Unity.Collections;

namespace SolidSpace.Profiling.Data
{
    public struct ProfilingResult
    {
        public List<string> names;
        public NativeArray<ushort> childIndexes;
        public NativeArray<ushort> siblingIndexes;
        public NativeArray<ushort> nameIndexes;
    }
}