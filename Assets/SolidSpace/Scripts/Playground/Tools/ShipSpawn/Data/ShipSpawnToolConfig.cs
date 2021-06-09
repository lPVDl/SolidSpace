using System;
using SolidSpace.Playground.Core;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ShipSpawn
{
    [Serializable]
    public class ShipSpawnToolConfig
    {
        public PlaygroundToolConfig ToolConfig => _toolConfig;
        public Texture2D ShipTexture => _shipTexture;
        
        [SerializeField] private PlaygroundToolConfig _toolConfig;
        [SerializeField] private Texture2D _shipTexture;
    }
}