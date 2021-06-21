using System;
using System.Collections.Generic;
using SolidSpace.GameCycle;
using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.UI
{
    internal class UIManager : IUIManager, IInitializable
    {
        public bool IsMouseOver => _hoveredElements > 0;
        
        private readonly UIConfig _config;
        private readonly List<IUIFactory> _factories;
        private readonly IUIEventManager _events;

        private Dictionary<Type, IUIFactory> _factoryStorage;
        private Dictionary<string, VisualElement> _rootContainers;
        private VisualElement _rootElement;
        private int _hoveredElements;

        public UIManager(UIConfig config, List<IUIFactory> factories, IUIEventManager events)
        {
            _config = config;
            _factories = factories;
            _events = events;
        }
        
        public void OnInitialize()
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

        public void AddToRoot(IUIElement view, string rootContainerName)
        {
            if (view is null) throw new ArgumentNullException(nameof(view));

            _events.Register<MouseEnterEvent>(view.Root, OnMouseEnter);
            _events.Register<MouseLeaveEvent>(view.Root, OnMouseLeave);

            var container = GetRootContainer(rootContainerName);
            container.Add(view.Root);
        }

        public void RemoveFromRoot(IUIElement view, string rootContainerName)
        {
            if (view is null) throw new ArgumentNullException(nameof(view));

            _events.Unregister<MouseEnterEvent>(view.Root, OnMouseEnter);
            _events.Unregister<MouseLeaveEvent>(view.Root, OnMouseLeave);
            
            var container = GetRootContainer(rootContainerName);
            container.Remove(view.Root);
        }

        private VisualElement GetRootContainer(string name)
        {
            if (!_rootContainers.TryGetValue(name, out var container))
            {
                throw new InvalidOperationException($"Container with name '{name}' was not found");
            }

            return container;
        }

        public void OnFinalize() { }
    }
}