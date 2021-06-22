using System;
using System.Collections.Generic;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Tools.SpawnPoint
{
    internal class SpawnPointTool : ISpawnPointTool
    {
        public VisualElement Root { get; set; }
        public IUIManager UIManager { get; set; }
        public IPointerTracker Pointer { get; set; }
        public GizmosHandle Gizmos { get; set; }
        public IStringField SpawnRadiusField { get; set; }
        public IStringField SpawnAmountField { get; set; }
        public IPlaygroundToolValueStorage ValueStorage { get; set; }
        public int SpawnRadius { get; set; }
        public int SpawnAmount { get; set; }
        
        public PositionGenerator PositionGenerator { get; set; }

        public IEnumerable<float2> OnUpdate()
        {
            var pointerPosition = Pointer.Position;

            if (UIManager.IsMouseOver)
            {
                yield break;
            }
            
            Gizmos.DrawWirePolygon(pointerPosition, SpawnRadius, 64, Color.yellow);

            var positions = PositionGenerator.IteratePositions(pointerPosition, SpawnRadius, SpawnAmount);
            
            foreach (var pos in positions)
            {
                Gizmos.DrawScreenSquare(pos, 6, Color.yellow);
            }
            
            if (!Pointer.ClickedThisFrame)
            {
                yield break;
            }
            
            foreach (var pos in positions)
            {
                yield return pos;
            }
        }

        public void OnActivate(bool isActive)
        {
            if (isActive)
            {
                SpawnRadius = (int) ValueStorage.GetValueOrDefault("InteractionRange");
                SpawnAmount = (int) ValueStorage.GetValueOrDefault("SpawnAmount");

                SpawnRadius = Math.Max(0, SpawnRadius);
                SpawnAmount = Math.Max(1, SpawnAmount);
                
                SpawnRadiusField.SetValue(SpawnRadius.ToString());
                SpawnAmountField.SetValue(SpawnAmount.ToString());
            }
            else
            {
                ValueStorage.SetValue("InteractionRange", SpawnRadius);
                ValueStorage.SetValue("SpawnAmount", SpawnAmount);
            }
        }
    }
}