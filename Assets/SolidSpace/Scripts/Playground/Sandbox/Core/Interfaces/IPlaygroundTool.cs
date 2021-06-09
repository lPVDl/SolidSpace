namespace SolidSpace.Playground.Sandbox.Core
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