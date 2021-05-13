using SolidSpace.GameCycle;

namespace SolidSpace.Entities
{
    public class EntityWorldTime : IEntityWorldTime, IController
    {
        public EControllerType ControllerType => EControllerType.EntityTime;
        
        public double ElapsedTime { get; private set; }
        
        public float DeltaTime { get; private set; }
        
        public void InitializeController()
        {
            
        }

        public void UpdateController()
        {
            var deltaTime = UnityEngine.Time.deltaTime;
            ElapsedTime += deltaTime;
            DeltaTime = deltaTime;
        }

        public void FinalizeController()
        {
            
        }
    }
}