using Unity.Entities;
using Unity.Mathematics;

namespace ECSTest.Scripts
{
    public class ShipRegenSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;

            Entities.ForEach((ref ShipData shipComponent) =>
            {
                var newHealth = shipComponent.health + shipComponent.healthRegen * deltaTime;
                shipComponent.health = math.min(shipComponent.maxHealth, newHealth);
            }).ScheduleParallel();
        }
    }
}