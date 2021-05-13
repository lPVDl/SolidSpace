namespace SolidSpace.GameCycle
{
    public interface IController
    {
        EControllerType ControllerType { get; }

        void InitializeController();
        
        void UpdateController();
        
        void FinalizeController();
    }
}