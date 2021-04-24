namespace SolidSpace
{
    public interface IUpdatable
    {
        EControllerType ControllerType { get; }

        void Update();
    }
}