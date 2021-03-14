using Unity.Entities;
using Unity.Transforms;

namespace ECSTest.Scripts
{
    public class ShipMovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            
            Entities.ForEach((ref Translation translation, ref ShipMovementData shipMovement) =>
            {
                var newY = translation.Value.y + shipMovement.movementSpeed * deltaTime;

                if (newY > 10)
                {
                    newY -= 20;
                }

                translation.Value.y = newY;
            }).ScheduleParallel();
        }
    }
}