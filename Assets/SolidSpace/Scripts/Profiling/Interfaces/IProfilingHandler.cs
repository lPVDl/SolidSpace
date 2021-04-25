namespace SolidSpace.Profiling.Interfaces
{
    public interface IProfilingHandler
    {
        void OnBeginSample(string name);

        void OnEndSample(string name);
    }
}