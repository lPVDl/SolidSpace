using SolidSpace.Playground.Sandbox.Core;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox.ShipSpawn
{
    internal class ShipSpawnTool : IPlaygroundTool
    {
        public Sprite Icon => _config.Icon;
        
        private readonly ShipSpawnToolConfig _config;

        public ShipSpawnTool(ShipSpawnToolConfig config)
        {
            _config = config;
        }
        
        public void Initialize()
        {
            
        }
        
        public void OnMouseClick(float2 worldPosition)
        {
            Debug.LogError("Caramba! " + worldPosition);
        }
        
        public void OnToolSelected()
        {
            
        }

        public void OnToolDeselected()
        {
            
        }

        public void FinalizeTool()
        {
            
        }
    }
}