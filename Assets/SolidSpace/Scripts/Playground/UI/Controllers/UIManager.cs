using System;
using System.Collections.Generic;
using SolidSpace.GameCycle;
using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.UI
{
    internal class UIManager : IUIManager, IController
    {
        public EControllerType ControllerType => EControllerType.UI;
        
        private readonly UIConfig _config;
        
        private Dictionary<string, VisualElement> _rootContainers;
        private VisualElement _rootElement;

        public UIManager(UIConfig config)
        {
            _config = config;
        }
        
        public void InitializeController()
        {
            var rootObject = new GameObject(nameof(UIManager));
            var document = rootObject.AddComponent<UIDocument>();
            document.panelSettings = _config.PanelSettings;
            
            _rootElement = _config.RootAsset.CloneTree();
            document.rootVisualElement.Add(_rootElement);
            
            _rootContainers = new Dictionary<string, VisualElement>();
            foreach (var containerName in _config.ContainerNames)
            {
                var containerElement = _rootElement.Query<VisualElement>(containerName).First();
                _rootContainers.Add(containerName, containerElement);
            }
        }
        
        public void UpdateController()
        {
            
        }
        
        public UIElementHandle CreateElement(UIPrefab prefab)
        {
            if (prefab is null) throw new ArgumentNullException(nameof(prefab));
            if (prefab.Asset is null) throw new InvalidOperationException($"{nameof(prefab)}.{nameof(prefab.Asset)} is null");

            return new UIElementHandle
            {
                element = prefab.Asset.CloneTree(),
                isValid = true
            };
        }

        public void AttachElementToRoot(UIElementHandle element, string rootContainerName)
        {
            if (!element.isValid) throw new ArgumentException(nameof(element));
            if (string.IsNullOrEmpty(rootContainerName)) throw new ArgumentException(nameof(rootContainerName));
            
            if (!_rootContainers.TryGetValue(rootContainerName, out var rootContainer))
            {
                throw new InvalidOperationException($"Container with name '{rootContainerName}' was not found");
            }
            
            rootContainer.Add(element.element);
        }


        public void FinalizeController()
        {
            
        }
    }
}