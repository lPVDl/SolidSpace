using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SolidSpace.GameCycle;
using SolidSpace.Playground.UI;
using Unity.Entities;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public class ComponentFilterMaster : IController, IComponentFilterMaster
    {
        public EControllerType ControllerType => EControllerType.UI;

        public IReadOnlyList<ComponentType> AllComponents => _allComponents;

        private readonly ComponentFilterMasterConfig _config;
        
        private Dictionary<ComponentType, int> _componentToIndex;
        private ComponentType[] _allComponents;
        private FilterState[] _defaultFilter;

        public ComponentFilterMaster(ComponentFilterMasterConfig config)
        {
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
        
        public void UpdateController()
        {
            
        }
        
        public FilterState[] CreateFilter()
        {
            var filter = new FilterState[_allComponents.Length];
            Array.Copy(_defaultFilter, filter, _defaultFilter.Length);
            return filter;
        }

        public void ModifyFilter(FilterState[] filter, ComponentType component, FilterState newState)
        {
            var index = _componentToIndex[component];
            filter[index] = newState;
        }

        public void SplitFilter(FilterState[] filter, IList<ComponentType> outWhitelist, IList<ComponentType> outBlacklist)
        {
            outWhitelist.Clear();
            outBlacklist.Clear();

            for (var i = 0; i < _allComponents.Length; i++)
            {
                var com = _allComponents[i];
                if (filter[i].state == ETagLabelState.Positive)
                {
                    outWhitelist.Add(com);
                    continue;
                }

                if (filter[i].state == ETagLabelState.Negative)
                {
                    outBlacklist.Add(com);
                }
            }
        }

        private IEnumerable<Type> IterateAllComponents()
        {
            var inter = typeof(IComponentData);

            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => t.IsValueType && inter.IsAssignableFrom(t))
                .Where(t => Regex.IsMatch(t.FullName, _config.FilterRegex));
        }

        public void FinalizeController()
        {
            
        }
    }
}