namespace SpaceSimulator.Runtime.Entities
{
    public interface IEntitySystem
    {
        public ESystemType SystemType { get; }

        public void Initialize();

        public void Update();

        public void FinalizeSystem();
    }
}