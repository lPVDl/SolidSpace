using SolidSpace.GameCycle;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using Unity.Mathematics;

namespace SolidSpace.Gizmos
{
    public class GizmosWindow : IInitializable, IUpdatable
    {
        private readonly IUIFactory _uiFactory;
        private readonly IUIManager _uiManager;
        private IToolWindow _window;

        public GizmosWindow(IUIFactory uiFactory, IUIManager uiManager)
        {
            _uiFactory = uiFactory;
            _uiManager = uiManager;
        }
        
        public void OnInitialize()
        {
            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Gizmos");
            _uiManager.AttachToRoot(_window, "ContainerB");

            var list = _uiFactory.CreateVerticalList();
            _window.AttachChild(list);

            for (var i = 10; i < 20; i++)
            {
                var button = _uiFactory.CreateTagLabel();
                button.SetLabel("Button_" + i);
                list.AttachItem(button);
            }
            
            list.SetSliderState(new int2(0, 100), new int2(10, 20));
        }
        
        public void OnUpdate()
        {
            
        }

        public void OnFinalize()
        {
            
        }
    }
}