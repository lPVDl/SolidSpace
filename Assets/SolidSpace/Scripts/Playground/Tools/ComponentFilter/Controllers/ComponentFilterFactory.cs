using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SolidSpace.Playground.UI;
using Unity.Entities;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public class ComponentFilterFactory : IComponentFilterFactory
    {
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly ComponentFilterFactoryConfig _config;

        public ComponentFilterFactory(IPlaygroundUIFactory uiFactory, ComponentFilterFactoryConfig config)
        {
            _uiFactory = uiFactory;
            _config = config;
        }

        public IComponentFilter Create()
        {
            var rawTypes = IterateAllComponents().ToArray();
            var allComponents = new ComponentType[rawTypes.Length];
            var filter = new FilterState[rawTypes.Length];
            var componentToIndex = new Dictionary<ComponentType, int>();
            for (var i = 0; i < rawTypes.Length; i++)
            {
                var component = (ComponentType) rawTypes[i];
                allComponents[i] = component;
                componentToIndex[component] = i;
                filter[i] = new FilterState
                {
                    isLocked = false,
                    state = ETagLabelState.Neutral
                };
            }

            var window = _uiFactory.CreateToolWindow();
            window.SetTitle("Components");

            var view = new ComponentFilter();
            view.AllComponents = allComponents;
            view.ComponentToIndex = componentToIndex;
            view.Filter = filter;
            view.Root = window.Root;
            
            var container = _uiFactory.CreateLayoutGrid();
            container.SetFlexDirection(FlexDirection.Row);
            container.SetFlexWrap(Wrap.Wrap);
            window.AttachChild(container);

            var tags = new ITagLabel[allComponents.Length];
            for (var i = 0; i < allComponents.Length; i++)
            {
                var state = filter[i];
                var tag = _uiFactory.CreateTagLabel();
                var labelIndex = i;
                var name = _config.NameConverter.Replace(rawTypes[i].Name);
                tag.SetLabel(name);
                tag.SetState(state.state);
                tag.SetLocked(state.isLocked);
                tag.Clicked += () => view.OnTagClicked(labelIndex);
                container.AttachChild(tag);
                tags[i] = tag;
            }
            view.Tags = tags;

            return view;
        }

        private IEnumerable<Type> IterateAllComponents()
        {
            var inter = typeof(IComponentData);
            
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => t.IsValueType && inter.IsAssignableFrom(t))
                .Where(t => _config.Filter.IsMatch(t.FullName));
        }
    }
}