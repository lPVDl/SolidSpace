using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Entities;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Tools.Eraser
{
    internal class EraserToolWindow
    {
        private readonly EraserToolConfig _config;
        private readonly IToolWindow _window;
        
        public EraserToolWindow(IPlaygroundUIFactory uiFactory, IUIManager uiManager, EraserToolConfig config)
        {
            _config = config;
            _window = uiFactory.CreateToolWindow();
            uiManager.AttachToRoot(_window, "ContainerA");
            _window.SetTitle("Components");

            var container = uiFactory.CreateLayoutGrid();
            container.SetFlexDirection(FlexDirection.Row);
            container.SetFlexWrap(Wrap.Wrap);
            _window.AttachChild(container);

            foreach (var com in IterateAllComponents())
            {
                var label = uiFactory.CreateTagLabel();
                var name = Regex.Replace(com.Name, _config.ComponentNameRegex, _config.ComponentNameSubstitution);
                label.SetLabel(name);
                label.SetState(ETagLabelState.Neutral);
                container.AttachChild(label);
            }
        }

        public void SetVisible(bool isVisible)
        {
            _window.SetVisible(isVisible);
        }
        
        private IEnumerable<Type> IterateAllComponents()
        {
            var inter = typeof(IComponentData);

            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => t.IsValueType && inter.IsAssignableFrom(t))
                .Where(t => Regex.IsMatch(t.FullName, _config.ComponentFilterRegex));
        }
    }
}