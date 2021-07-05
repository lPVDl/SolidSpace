using System;
using SolidSpace.Playground.Core;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;

namespace SolidSpace.Playground.Tools.Spawn
{
    internal class SpawnTool : ISpawnTool
    {
        public IToolWindow Window { get; set; }
        public IUIManager UIManager { get; set; }
        public IPointerTracker Pointer { get; set; }
        public IStringField SpawnRadiusField { get; set; }
        public IStringField SpawnAmountField { get; set; }
        public IPlaygroundToolValueStorage ValueStorage { get; set; }
        public int SpawnRadius { get; set; }
        public int SpawnAmount { get; set; }
        public ISpawnToolHandler Handler { get; set; }
        public PositionGenerator PositionGenerator { get; set; }
        
        public IPlaygroundUIManager PlaygroundUI { get; set; }

        public void OnUpdate()
        {
            var pointerPosition = Pointer.Position;

            if (UIManager.IsMouseOver)
            {
                return;
            }
            
            Handler.OnDrawSpawnCircle(pointerPosition, SpawnRadius);

            var positions = PositionGenerator.IteratePositions(pointerPosition, SpawnRadius, SpawnAmount);
            
            foreach (var pos in positions)
            {
                Handler.OnSpawnEvent(new SpawnEventData
                {
                    eventType = ESpawnEventType.Preview,
                    position = pos
                });
            }
            
            if (!Pointer.ClickedThisFrame)
            {
                return;
            }
            
            foreach (var pos in positions)
            {
                Handler.OnSpawnEvent(new SpawnEventData
                {
                    eventType = ESpawnEventType.Place,
                    position = pos
                });
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
            
            PlaygroundUI.SetElementVisible(Window, isActive);
        }
    }
}