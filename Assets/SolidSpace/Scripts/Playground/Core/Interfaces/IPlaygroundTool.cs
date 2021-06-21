namespace SolidSpace.Playground.Core
{
    public interface IPlaygroundTool
    {
        PlaygroundToolConfig Config { get; }
        
        void OnInitialize();
        void OnUpdate();
        void OnActivate(bool isActive);
        void OnFinalize();
    }
}