using SpaceSimulator.Enums;

namespace SpaceSimulator.Interfaces
{
    public interface IInitializable
    {
        public EControllerType ControllerType { get; }

        void Initialize();
    }
}