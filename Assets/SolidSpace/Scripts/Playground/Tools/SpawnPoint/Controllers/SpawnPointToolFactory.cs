using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using UnityEngine;

namespace SolidSpace.Playground.Tools.SpawnPoint
{
    internal class SpawnPointToolFactory : ISpawnPointToolFactory
    {
        private readonly IUIManager _uiManager;
        private readonly IPointerTracker _pointer;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly IGizmosManager _gizmosManager;
        private readonly IPlaygroundToolValueStorage _valueStorage;

        public SpawnPointToolFactory(IUIManager uiManager, IPointerTracker pointer, IPlaygroundUIFactory uiFactory,
            IGizmosManager gizmosManager, IPlaygroundToolValueStorage valueStorage)
        {
            _uiManager = uiManager;
            _pointer = pointer;
            _uiFactory = uiFactory;
            _gizmosManager = gizmosManager;
            _valueStorage = valueStorage;
        }
        
        public ISpawnPointTool Create()
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

            var tool = new SpawnPointTool
            {
                UIManager = _uiManager,
                Pointer = _pointer,
                Root = window.Root,
                SpawnRadiusField = radiusField,
                SpawnAmountField = amountField,
                SpawnAmount = 1,
                SpawnRadius = 0,
                ValueStorage = _valueStorage,
                PositionGenerator = new PositionGenerator()
            };

            tool.Gizmos = _gizmosManager.GetHandle(tool, Color.yellow);

            radiusField.ValueChanged += () => tool.SpawnRadius = int.Parse(tool.SpawnRadiusField.Value);
            amountField.ValueChanged += () => tool.SpawnAmount = int.Parse(tool.SpawnAmountField.Value);

            return tool;
        }
    }
}