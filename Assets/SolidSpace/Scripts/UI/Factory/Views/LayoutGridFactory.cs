using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    internal class LayoutGridFactory : AUIViewFactory<LayoutGrid>
    {
        protected override LayoutGrid Create(VisualElement root)
        {
            return new LayoutGrid
            {
                Root = root
            };
        }
    }
}