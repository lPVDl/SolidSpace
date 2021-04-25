namespace SolidSpace
{
    public interface IController
    {
        EControllerType ControllerType { get; }

        void Initialize();
        
        void Update();
        
        void FinalizeObject();
    }
}