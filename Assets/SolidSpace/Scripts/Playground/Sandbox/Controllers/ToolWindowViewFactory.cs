using SolidSpace.Playground.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Sandbox
{
    public class ToolWindowViewFactory : AUIFactory<IToolWindowView>, IUIPrefabValidator<IToolWindowView>
    {
        private const string AttachPoint = "AttachPoint";
        
        private readonly VisualElement _cloneTarget;

        public ToolWindowViewFactory()
        {
            _cloneTarget = new VisualElement();
        }
        
        protected override IToolWindowView Create(VisualElement source)
        {
            return new ToolWindowView
            {
                Source = source,
                AttachPoint = source.Query<VisualElement>(AttachPoint).First()
            };
        }

        public string Validate(UIPrefab<IToolWindowView> data)
        {
            if (data.Asset is null)
            {
                return $"'{nameof(data.Asset)} is null'";
            }

            data.Asset.CloneTree(_cloneTarget);

            var query = _cloneTarget.Query<VisualElement>(AttachPoint).First();
            if (query is null)
            {
                return $"Asset '{data.Asset.name}' does not have child element with name '{AttachPoint}'";
            }

            return string.Empty;
        }
    }
}