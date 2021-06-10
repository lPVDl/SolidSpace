using SolidSpace.Playground.UI;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Tools.Eraser
{
    internal class EraserToolWindow
    {
        private readonly IToolWindow _window;
        
        public EraserToolWindow(IPlaygroundUIFactory uiFactory, IUIManager uiManager)
        {
            _window = uiFactory.CreateToolWindow();
            uiManager.AttachToRoot(_window, "ContainerA");
            _window.SetTitle("Filter");

            var container = uiFactory.CreateLayoutGrid();
            container.SetFlexDirection(FlexDirection.Row);
            container.SetFlexWrap(Wrap.Wrap);
            _window.AttachChild(container);

            var testNames = new string[]
            {
                "Velocity",
                "ParticleEmitter",
                "Collider",
                "Raycaster",
                "Vacuum",
                "Radar",
                "WarpGate",
                "LowOrbitalIonCannon"
            };

            foreach (var name in testNames)
            {
                var label = uiFactory.CreateTagLabel();
                label.SetLabel(name);
                label.SetState(ETagLabelState.Neutral);
                
                container.AttachChild(label);
            }
        }

        public void SetVisible(bool isVisible)
        {
            _window.SetVisible(isVisible);
        }
    }
}