using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SolidSpace.Entities.Components;
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
            public ComponentType type;
        }
        
        private readonly EraserToolConfig _config;
        private readonly IEntityByPositionSearchSystem _searchSystem;
        private readonly IToolWindow _window;
        private readonly LabelInfo[] _labels;
        
        public EraserToolWindow(IPlaygroundUIFactory uiFactory, IUIManager uiManager, EraserToolConfig config,
            IEntityByPositionSearchSystem searchSystem)
        {
            _config = config;
            _searchSystem = searchSystem;
            _window = uiFactory.CreateToolWindow();
            uiManager.AttachToRoot(_window, "ContainerA");
            _window.SetTitle("Components");

            var container = uiFactory.CreateLayoutGrid();
            container.SetFlexDirection(FlexDirection.Row);
            container.SetFlexWrap(Wrap.Wrap);
            _window.AttachChild(container);

            var components = IterateAllComponents().ToArray();
            _labels = new LabelInfo[components.Length];

            var positionComponentType = (ComponentType) typeof(PositionComponent);
            
            for (var i = 0; i < _labels.Length; i++)
            {
                var type = components[i];
                var view = uiFactory.CreateTagLabel();
                var labelIndex = i;
                var componentType = (ComponentType) type;
                var isPositionComponent = componentType == positionComponentType;
                
                var info = new LabelInfo
                {
                    view = view,
                    state = isPositionComponent ? ETagLabelState.Positive : ETagLabelState.Neutral,
                    isLocked = isPositionComponent,
                    type = componentType
                };
                _labels[i] = info;
                
                var name = Regex.Replace(type.Name, _config.ComponentNameRegex, _config.ComponentNameSubstitution);
                view.SetLabel(name);
                view.SetState(info.state);
                view.SetLocked(info.isLocked);
                view.Clicked += () => OnLabelClicked(labelIndex);
                container.AttachChild(view);
            }
            
            _searchSystem.SetQuery(BuildQuery());
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
            
            _searchSystem.SetQuery(BuildQuery());
        }

        private EntityQueryDesc BuildQuery()
        {
            return new EntityQueryDesc
            {
                All = _labels.Where(l => l.state == ETagLabelState.Positive).Select(l => l.type).ToArray(),
                None = _labels.Where(l => l.state == ETagLabelState.Negative).Select(l => l.type).ToArray()
            };
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