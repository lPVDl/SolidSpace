namespace SpaceSimulator.Entities
{
    public class EntityWorldTime : IEntityWorldTime, IEntitySystem
    {
        public ESystemType SystemType => ESystemType.Time;
        
        public double ElapsedTime { get; private set; }
        
        public float DeltaTime { get; private set; }
        
        public void Initialize()
        {
            
        }

        public void Update()
        {
            var deltaTime = UnityEngine.Time.deltaTime;
            ElapsedTime += deltaTime;
            DeltaTime = deltaTime;
        }

        public void FinalizeSystem()
        {
            
        }
    }
}