using System;
using System.Collections.Generic;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Entities;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public interface IComponentFilter : IUIElement
    {
        event Action FilterModified;

        IEnumerable<ComponentType> GetTagsWithState(ETagLabelState state);
    }
}