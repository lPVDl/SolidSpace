namespace SolidSpace
{
    public interface IInitializable
    {
        public EControllerType ControllerType { get; }

        void Initialize();
    }
}