using System.Collections.Generic;
using SolidSpace.Profiling.Data;

namespace SolidSpace.Profiling.Interfaces
{
    public interface IProfilingTree
    {
        public IReadOnlyList<ProfilingNode> Nodes { get; }
        
        public IReadOnlyList<string> Names { get; }
    }
}