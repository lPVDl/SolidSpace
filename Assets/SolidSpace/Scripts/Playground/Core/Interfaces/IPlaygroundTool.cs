namespace SolidSpace.Playground.Core
{
    public interface IPlaygroundTool
    {
        PlaygroundToolConfig Config { get; }
        
        void InitializeTool();
        void Update();
        void OnToolActivation();
        void OnToolDeactivation();
        void FinalizeTool();
    }
}