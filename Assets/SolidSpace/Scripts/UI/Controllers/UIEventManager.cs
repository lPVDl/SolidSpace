using System;
using System.Collections.Generic;
using SolidSpace.GameCycle;
using UnityEngine.UIElements;

namespace SolidSpace.UI
{
    internal class UIEventManager : IUIEventManager, IInitializable, IUpdatable
    {
        private struct HandlerId
        {
            public object target;
            public Type eventType;
        }

        private struct EventData
        {
            public IUICallbackHandler handler;
            public object data;

        }

        private Dictionary<HandlerId, IUICallbackHandler> _handlers;
        private List<EventData> _events;

        public void OnInitialize()
        {
            _handlers = new Dictionary<HandlerId, IUICallbackHandler>();
            _events = new List<EventData>();
        }
        
        public void OnUpdate()
        {
            for (var i = 0; i < _events.Count; i++)
            {
                var info = _events[i];
                info.handler.Invoke(info.data);
            }
            
            _events.Clear();
        }

        public void Register<T>(VisualElement element, Action<T> handler) where T : EventBase<T>, new()
        {
            var id = new HandlerId
            {
                target = element,
                eventType = typeof(T)
            };

            if (_handlers.TryGetValue(id, out _))
            {
                var message = $"Handler ({typeof(T).Name}) for '{element.name}' was already registered";
                throw new InvalidOperationException(message);
            }
            
            _handlers.Add(id, new IUICallbackHandler<T>(handler));
            
            element.RegisterCallback<T>(ProcessEvent);
        }

        public void Unregister<T>(VisualElement element, Action<T> handler) where T : EventBase<T>, new()
        {
            var id = new HandlerId
            {
                target = element,
                eventType = typeof(T)
            };

            if (!_handlers.Remove(id))
            {
                var message = $"Handler ({typeof(T).Name} was not registered";
                throw new InvalidOperationException(message);
            }

            element.UnregisterCallback<T>(ProcessEvent);
        }

        private void ProcessEvent<T>(T eventData) where T : EventBase<T>, new()
        {
            if (eventData is MouseDownEvent)
            {
                eventData.StopPropagation();
            }
            
            var id = new HandlerId
            {
                target = eventData.currentTarget,
                eventType = typeof(T)
            };

            if (!_handlers.TryGetValue(id, out var handler))
            {
                var message = $"Handler for ({typeof(T)} was not found";
                throw new InvalidOperationException(message);
            }

            _events.Add(new EventData
            {
                handler = handler,
                data = eventData,
            });
        }

        public void OnFinalize() { }
    }
}