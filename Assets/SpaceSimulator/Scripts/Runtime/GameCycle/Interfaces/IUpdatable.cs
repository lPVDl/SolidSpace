namespace SpaceSimulator.Runtime
{
    public interface IUpdatable
    {
        EControllerType ControllerType { get; }

        void Update();
    }
}