using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    public static class UIAssetValidator
    {
        private static readonly VisualElement DummyElement;

        static UIAssetValidator()
        {
            DummyElement = new VisualElement();
        }

        public static bool TreeHasChildElement<T>(VisualTreeAsset treeAsset, string childName, out string errorMessage) 
            where T : VisualElement
        {
            errorMessage = string.Empty;

            treeAsset.CloneTree(DummyElement);
            var child = DummyElement.Query<T>(childName).First();
            if (child is null)
            {
                errorMessage = $"Tree '{treeAsset.name}' does not have child '{childName}'({nameof(T)})";
                return false;
            }

            return true;
        }
    }
}