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
        private struct LabelInfo
        {
            public ITagLabel view;
            public bool isLocked;
            public ETagLabelState state;
        }
        
        private readonly EraserToolConfig _config;
        private readonly IToolWindow _window;
        private readonly LabelInfo[] _labels;
        
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

            var components = IterateAllComponents().ToArray();
            _labels = new LabelInfo[components.Length];

            for (var i = 0; i < _labels.Length; i++)
            {
                var com = components[i];
                var label = uiFactory.CreateTagLabel();
                var name = Regex.Replace(com.Name, _config.ComponentNameRegex, _config.ComponentNameSubstitution);
                label.SetLabel(name);
                label.SetState(ETagLabelState.Neutral);
                container.AttachChild(label);
                var labelIndex = i;
                label.Clicked += () => OnLabelClicked(labelIndex);

                _labels[i] = new LabelInfo
                {
                    view = label,
                    state = ETagLabelState.Neutral,
                    isLocked = false,
                };
            }
        }

        public void SetVisible(bool isVisible)
        {
            _window.SetVisible(isVisible);
        }

        private void OnLabelClicked(int index)
        {
            var labelInfo = _labels[index];
            if (labelInfo.isLocked)
            {
                return;
            }

            labelInfo.state = AdvanceLabelState(labelInfo.state);
            labelInfo.view.SetState(labelInfo.state);
            _labels[index] = labelInfo;
        }

        private ETagLabelState AdvanceLabelState(ETagLabelState state)
        {
            return state switch
            {
                ETagLabelState.Neutral => ETagLabelState.Positive,
                ETagLabelState.Positive => ETagLabelState.Negative,
                ETagLabelState.Negative => ETagLabelState.Neutral,
                
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
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