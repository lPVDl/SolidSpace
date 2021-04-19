namespace SpaceSimulator
{
    public interface IInitializable
    {
        public EControllerType ControllerType { get; }

        void Initialize();
    }
}