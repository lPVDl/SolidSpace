namespace SpaceSimulator.Entities
{
    public interface IEntityWorldTime
    {
        public double ElapsedTime { get; }
        
        public float DeltaTime { get; }
    }
}