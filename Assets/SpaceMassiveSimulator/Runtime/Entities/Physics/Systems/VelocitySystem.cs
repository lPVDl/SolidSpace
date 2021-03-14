using Unity.Entities;

namespace SpaceMassiveSimulator.Runtime.Entities.Physics
{
    public class VelocitySystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            
            Entities.ForEach((ref PositionComponent position, ref VelocityComponent velocity) =>
            {
                position.value += velocity.value * deltaTime;
            }).ScheduleParallel();
        }
    }
}