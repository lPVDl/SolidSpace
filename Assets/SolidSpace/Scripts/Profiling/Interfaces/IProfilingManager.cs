using SolidSpace.Profiling.Data;

namespace SolidSpace.Profiling.Interfaces
{
    public interface IProfilingManager
    {
        ProfilingResultReadOnly Result { get; }
        
        ProfilingHandle GetHandle(object owner);
    }
}