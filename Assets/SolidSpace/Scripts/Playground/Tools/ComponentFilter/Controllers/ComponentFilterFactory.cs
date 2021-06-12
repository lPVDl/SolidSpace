using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SolidSpace.GameCycle;
using SolidSpace.Playground.UI;
using Unity.Entities;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public class ComponentFilterFactory : IComponentFilterFactory, IController
    {
        public EControllerType ControllerType => EControllerType.UI;
        
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly ComponentFilterFactoryConfig _config;
        
        private ComponentType[] _allComponents;
        private FilterState[] _defaultFilter;
        private Dictionary<ComponentType, int> _componentToIndex;

        public ComponentFilterFactory(IPlaygroundUIFactory uiFactory, ComponentFilterFactoryConfig config)
        {
            _uiFactory = uiFactory;
            _config = config;
        }
        
        public void InitializeController()
        {
            var rawTypes = IterateAllComponents().ToArray();
            _allComponents = new ComponentType[rawTypes.Length];
            _defaultFilter = new FilterState[rawTypes.Length];
            _componentToIndex = new Dictionary<ComponentType, int>();
            for (var i = 0; i < rawTypes.Length; i++)
            {
                var component = (ComponentType) rawTypes[i];
                _allComponents[i] = component;
                _componentToIndex[component] = i;
                _defaultFilter[i] = new FilterState
                {
                    isLocked = false,
                    state = ETagLabelState.Neutral
                };
            }
        }
        
        public IComponentFilter Create()
        {
            var view = new Views.ComponentFilter();
            view.AllComponents = _allComponents;
            view.ComponentToIndex = _componentToIndex;
            
            var container = _uiFactory.CreateLayoutGrid();
            container.SetFlexDirection(FlexDirection.Row);
            container.SetFlexWrap(Wrap.Wrap);
            view.Root = container.Root;

            var filter = CreateFilter();
            view.Filter = filter;
            
            var tags = new ITagLabel[_allComponents.Length];
            for (var i = 0; i < _allComponents.Length; i++)
            {
                var type = _allComponents[i];
                var state = filter[i];
                var tag = _uiFactory.CreateTagLabel();
                var labelIndex = i;
                var name = Regex.Replace(type.ToString(), _config.NameRegex, _config.NameSubstitution);
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
        
        private FilterState[] CreateFilter()
        {
            var filter = new FilterState[_allComponents.Length];
            Array.Copy(_defaultFilter, filter, _defaultFilter.Length);
            return filter;
        }
        
        private IEnumerable<Type> IterateAllComponents()
        {
            var inter = typeof(IComponentData);
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => t.IsValueType && inter.IsAssignableFrom(t))
                .Where(t => Regex.IsMatch(t.FullName, _config.FilterRegex));
        }

        public void UpdateController() { }
        public void FinalizeController() { }
    }
}