using System;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox.ShipSpawn
{
    [Serializable]
    public class ShipSpawnToolConfig
    {
        public Sprite Icon => _icon;
        
        [SerializeField] private Sprite _icon;
    }
}