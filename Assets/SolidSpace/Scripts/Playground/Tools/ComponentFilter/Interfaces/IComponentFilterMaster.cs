using System.Collections.Generic;
using Unity.Entities;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public interface IComponentFilterMaster
    {
        IReadOnlyList<ComponentType> AllComponents { get; }
        FilterState[] CreateFilter();
        void ModifyFilter(FilterState[] filter, ComponentType component, FilterState newState);
        void SplitFilter(FilterState[] filter, IList<ComponentType> outWhitelist, IList<ComponentType> outBlacklist);
    }
}