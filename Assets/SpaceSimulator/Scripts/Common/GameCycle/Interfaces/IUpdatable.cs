using SpaceSimulator.Enums;

namespace SpaceSimulator.Interfaces
{
    public interface IUpdatable
    {
        EControllerType ControllerType { get; }

        void Update();
    }
}