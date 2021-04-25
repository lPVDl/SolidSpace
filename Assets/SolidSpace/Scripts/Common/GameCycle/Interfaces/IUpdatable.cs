namespace SolidSpace
{
    // TODO: Make IController, AController
    public interface IUpdatable
    {
        EControllerType ControllerType { get; }

        void Update();
    }
}