using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    internal class LayoutGridFactory : AUIFactory<LayoutGrid>
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