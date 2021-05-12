namespace SolidSpace.GameCycle
{
    public interface IController
    {
        EControllerType ControllerType { get; }

        void Initialize();
        
        void Update();
        
        void FinalizeObject();
    }
}