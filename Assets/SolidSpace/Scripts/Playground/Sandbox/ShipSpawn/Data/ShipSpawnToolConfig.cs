using System;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox.ShipSpawn
{
    [Serializable]
    public class ShipSpawnToolConfig
    {
        public Sprite ToolIcon => _toolIcon;
        public Texture2D ShipTexture => _shipTexture;
        
        [SerializeField] private Sprite _toolIcon;
        [SerializeField] private Texture2D _shipTexture;
    }
}