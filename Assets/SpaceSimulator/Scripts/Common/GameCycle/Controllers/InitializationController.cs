using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceSimulator
{
    public class InitializationController : Zenject.IInitializable
    {
        private readonly GameCycleConfig _config;
        private readonly List<IInitializable> _initializables;

        public InitializationController(List<IInitializable> initializables, GameCycleConfig config)
        {
            _initializables = initializables;
            _config = config;
        }

        public void Initialize()
        {
            var order = new Dictionary<EControllerType, int>();
            for (var i = 0; i < _config.InvocationOrder.Count; i++)
            {
                order[_config.InvocationOrder[i]] = i;
            }

            var unordered = _initializables.Where(i => !order.ContainsKey(i.ControllerType)).ToList();
            
            if (unordered.Any())
            {
                foreach (var controller in unordered)
                {
                    var message = $"{controller.GetType()} ({controller.ControllerType}) is missing in initialization order list.";
                    Debug.LogError(message);
                }
                
                throw new InvalidOperationException("Failed to create initialization order.");
            }
            
            foreach (var item in _initializables.OrderBy(i => order[i.ControllerType]))
            {
                item.Initialize();
            }
        }
    }
}