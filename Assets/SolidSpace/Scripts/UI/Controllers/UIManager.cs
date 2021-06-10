using System;
using System.Collections.Generic;
using SolidSpace.GameCycle;
using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.UI
{
    internal class UIManager : IUIManager, IController
    {
        public EControllerType ControllerType => EControllerType.UI;

        public bool IsMouseOver => _hoveredElements > 0;
        
        private readonly UIConfig _config;
        private readonly List<IUIFactory> _factories;

        private Dictionary<Type, IUIFactory> _factoryStorage;
        private Dictionary<string, VisualElement> _rootContainers;
        private VisualElement _rootElement;
        private int _hoveredElements;

        public UIManager(UIConfig config, List<IUIFactory> factories)
        {
            _config = config;
            _factories = factories;
        }
        
        public void InitializeController()
        {
            _factoryStorage = new Dictionary<Type, IUIFactory>();
            foreach (var factory in _factories)
            {
                var elementType = factory.ViewType;
                if (_factoryStorage.TryGetValue(elementType, out var existingFactory))
                {
                    var message = $"More than one factory is defined for '{elementType}'. ";
                    message += $"'{factory.GetType()}' conflicts with '{existingFactory.GetType()}'";
                    throw new InvalidOperationException(message);
                }

                _factoryStorage[elementType] = factory;
            }
            
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
        
        private void OnMouseEnter(MouseEnterEvent e)
        {
            _hoveredElements++;
        }

        private void OnMouseLeave(MouseLeaveEvent e)
        {
            _hoveredElements--;
        }

        public T Instantiate<T>(UIPrefab<T> prefab) where T : class, IUIElement
        {
            if (prefab is null) throw new ArgumentNullException(nameof(prefab));
            if (prefab.Asset is null) throw new ArgumentNullException($"{nameof(prefab)}.{nameof(prefab.Asset)} is null");
            
            if (!_factoryStorage.TryGetValue(typeof(T), out var factory))
            {
                throw new InvalidOperationException($"{nameof(IUIFactory)} for type {typeof(T)} was not found");
            }

            return (T) factory.Create(prefab.Asset.CloneTree());
        }

        public void AttachToRoot(IUIElement view, string rootContainerName)
        {
            if (view is null) throw new ArgumentNullException(nameof(view));
            
            if (!_rootContainers.TryGetValue(rootContainerName, out var rootContainer))
            {
                throw new InvalidOperationException($"Container with name '{rootContainerName}' was not found");
            }
            
            view.Source.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            view.Source.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

            rootContainer.Add(view.Source);
        }

        public void FinalizeController()
        {
            
        }
    }
}