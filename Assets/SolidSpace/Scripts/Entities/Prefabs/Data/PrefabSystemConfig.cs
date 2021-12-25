using System;
using UnityEngine;

namespace SolidSpace.Entities.Prefabs
{
    [Serializable]
    internal class PrefabSystemConfig
    {
        public Texture2D ShipTexture => _shipTexture;
        
        [SerializeField] private Texture2D _shipTexture;
    }
}