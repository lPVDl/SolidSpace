namespace SpaceSimulator.Entities.EntityWorld
{
    public interface IEntityWorldTime
    {
        public double ElapsedTime { get; }
        
        public float DeltaTime { get; }
    }
}