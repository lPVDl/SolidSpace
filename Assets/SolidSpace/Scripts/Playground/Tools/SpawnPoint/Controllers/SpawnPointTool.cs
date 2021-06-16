using System.Collections.Generic;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SolidSpace.Playground.Tools.SpawnPoint
{
    internal class SpawnPointTool : ISpawnPointTool
    {
        public IUIManager UIManager { get; set; }
        public IPointerTracker Pointer { get; set; }
        public IToolWindow Window { get; set; }
        public GizmosHandle Gizmos { get; set; }
        public IStringField SpawnRadiusField { get; set; }
        public IStringField SpawnAmountField { get; set; }
        
        public int SpawnRadius { get; set; }
        
        public int SpawnAmount { get; set; }

        public IEnumerable<float2> Update()
        {
            var pointerPosition = Pointer.Position;
            
            Gizmos.DrawPolygon(pointerPosition, SpawnRadius, 32, Color.yellow);
            
            if (UIManager.IsMouseOver || !Pointer.ClickedThisFrame)
            {
                yield break;
            }

            for (var i = 0; i < SpawnAmount; i++)
            {
                var randomOffset = Random.insideUnitCircle * SpawnRadius;

                yield return pointerPosition + new float2(randomOffset.x, randomOffset.y);
            }
        }

        public void SetEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                UIManager.AddToRoot(Window, "ContainerA");
            }
            else
            {
                UIManager.RemoveFromRoot(Window, "ContainerA");
            }
        }
    }
}