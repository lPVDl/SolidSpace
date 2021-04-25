using SolidSpace.Profiling.Controllers;
using SolidSpace.Profiling.Data;

namespace SolidSpace.Profiling.Interfaces
{
    public interface IProfilingManager
    {
        ProfilingTreeReader Reader { get; }
        
        ProfilingHandle GetHandle(object owner);
    }
}