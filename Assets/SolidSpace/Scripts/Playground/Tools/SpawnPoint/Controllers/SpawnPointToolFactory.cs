using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.UI;
using SolidSpace.UI;

namespace SolidSpace.Playground.Tools.SpawnPoint
{
    internal class SpawnPointToolFactory : ISpawnPointToolFactory
    {
        private readonly IUIManager _uiManager;
        private readonly IPointerTracker _pointer;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly IGizmosManager _gizmosManager;

        public SpawnPointToolFactory(IUIManager uiManager, IPointerTracker pointer, IPlaygroundUIFactory uiFactory,
            IGizmosManager gizmosManager)
        {
            _uiManager = uiManager;
            _pointer = pointer;
            _uiFactory = uiFactory;
            _gizmosManager = gizmosManager;
        }
        
        public ISpawnPointTool Create()
        {
            var window = _uiFactory.CreateToolWindow();
            window.SetTitle("Spawn");

            var radiusField = _uiFactory.CreateStringField();
            radiusField.SetLabel("Radius");
            radiusField.SetValue("0");
            radiusField.SetValueCorrectionBehaviour(new MinMaxIntRangeBehaviour(0, 1000));
            window.AttachChild(radiusField);

            var amountField = _uiFactory.CreateStringField();
            amountField.SetLabel("Amount");
            amountField.SetValue("1");
            amountField.SetValueCorrectionBehaviour(new MinMaxIntRangeBehaviour(1, 100000));
            window.AttachChild(amountField);

            var tool = new SpawnPointTool
            {
                UIManager = _uiManager,
                Pointer = _pointer,
                Root = window.Root,
                SpawnRadiusField = radiusField,
                SpawnAmountField = amountField,
                SpawnAmount = 1,
                SpawnRadius = 0
            };

            tool.Gizmos = _gizmosManager.GetHandle(tool);

            radiusField.ValueChanged += () => tool.SpawnRadius = int.Parse(tool.SpawnRadiusField.Value);
            amountField.ValueChanged += () => tool.SpawnAmount = int.Parse(tool.SpawnAmountField.Value);

            return tool;
        }
    }
}