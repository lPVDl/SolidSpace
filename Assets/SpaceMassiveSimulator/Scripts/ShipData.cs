using Unity.Entities;

namespace ECSTest.Scripts
{
    public struct ShipData : IComponentData
    {
        public float health;
        public float maxHealth;
        public float healthRegen;
    }
}