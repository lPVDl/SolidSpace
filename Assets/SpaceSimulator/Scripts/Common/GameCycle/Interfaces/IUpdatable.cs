namespace SpaceSimulator
{
    public interface IUpdatable
    {
        EControllerType ControllerType { get; }

        void Update();
    }
}