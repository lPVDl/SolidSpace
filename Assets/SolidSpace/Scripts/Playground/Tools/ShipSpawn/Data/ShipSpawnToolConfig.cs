using System;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ShipSpawn
{
    [Serializable]
    public class ShipSpawnToolConfig
    {
        public Texture2D ShipTexture => _shipTexture;
        
        [SerializeField] private Texture2D _shipTexture;
    }
}