using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using UnityEngine;

namespace SolidSpace.Playground.Tools.Spawn
{
    internal class SpawnToolFactory : ISpawnToolFactory
    {
        private readonly IUIManager _uiManager;
        private readonly IPointerTracker _pointer;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly IPlaygroundToolValueStorage _valueStorage;
        private readonly IPlaygroundUIManager _playgroundUI;

        public SpawnToolFactory(IUIManager uiManager, IPointerTracker pointer, IPlaygroundUIFactory uiFactory,
            IPlaygroundToolValueStorage valueStorage, IPlaygroundUIManager playgroundUI)
        {
            _uiManager = uiManager;
            _pointer = pointer;
            _uiFactory = uiFactory;
            _valueStorage = valueStorage;
            _playgroundUI = playgroundUI;
        }
        
        public ISpawnTool Create(ISpawnToolHandler handler)
        {
            var window = _uiFactory.CreateToolWindow();
            window.SetTitle("Spawn");

            var radiusField = _uiFactory.CreateStringField();
            radiusField.SetLabel("Radius");
            radiusField.SetValue("0");
            radiusField.SetValueCorrectionBehaviour(new IntMaxBehaviour(0));
            window.AttachChild(radiusField);

            var amountField = _uiFactory.CreateStringField();
            amountField.SetLabel("Amount");
            amountField.SetValue("1");
            amountField.SetValueCorrectionBehaviour(new IntMaxBehaviour(1));
            window.AttachChild(amountField);

            var tool = new SpawnTool
            {
                UIManager = _uiManager,
                Pointer = _pointer,
                Window = window,
                SpawnRadiusField = radiusField,
                SpawnAmountField = amountField,
                SpawnAmount = 1,
                SpawnRadius = 0,
                ValueStorage = _valueStorage,
                PositionGenerator = new PositionGenerator(),
                Handler = handler,
                PlaygroundUI = _playgroundUI
            };

            radiusField.ValueChanged += () => tool.SpawnRadius = int.Parse(tool.SpawnRadiusField.Value);
            amountField.ValueChanged += () => tool.SpawnAmount = int.Parse(tool.SpawnAmountField.Value);

            return tool;
        }
    }
}