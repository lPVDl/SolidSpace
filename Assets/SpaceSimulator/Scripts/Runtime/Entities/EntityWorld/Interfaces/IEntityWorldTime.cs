namespace SpaceSimulator.Runtime.Entities
{
    public interface IEntityWorldTime
    {
        public double ElapsedTime { get; }
        
        public float DeltaTime { get; }
    }
}