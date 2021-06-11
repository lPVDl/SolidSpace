using System;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public interface IComponentFilterTool
    {
        event Action FilterModified;
        
        void SetEnabled(bool isEnabled);

        void SetFilter(FilterState[] newFilter);

        void GetFilter(FilterState[] output);
    }
}