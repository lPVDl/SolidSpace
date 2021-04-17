namespace SpaceSimulator.Runtime
{
    public interface IInitializable
    {
        public EControllerType ControllerType { get; }

        void Initialize();
    }
}