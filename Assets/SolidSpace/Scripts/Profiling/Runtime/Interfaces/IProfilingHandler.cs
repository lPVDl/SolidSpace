namespace SolidSpace.Profiling
{
    public interface IProfilingHandler
    {
        void OnBeginSample(string name);

        void OnEndSample(string name);
    }
}