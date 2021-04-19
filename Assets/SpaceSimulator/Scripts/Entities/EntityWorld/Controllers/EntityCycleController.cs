using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceSimulator.Entities.EntityWorld
{
    public class EntityCycleController : IInitializable, IUpdatable, IFinalazable
    {
        public EControllerType ControllerType => EControllerType.Common;

        private readonly EntityCycleConfig _config;

        private List<IEntitySystem> _systems;

        public EntityCycleController(List<IEntitySystem> systems, EntityCycleConfig config)
        {
            _systems = systems;
            _config = config;
        }
        
        public void Initialize()
        {
            var order = new Dictionary<ESystemType, int>();
            for (var i = 0; i < _config.InvocationOrder.Count; i++)
            {
                order[_config.InvocationOrder[i]] = i;
            }
            
            var unordered = _systems.Where(i => !order.ContainsKey(i.SystemType)).ToList();
            
            if (unordered.Any())
            {
                foreach (var controller in unordered)
                {
                    var message = $"{controller.GetType()} ({controller.SystemType}) is missing in update order list.";
                    Debug.LogError(message);
                }
                
                throw new InvalidOperationException("Failed to create update order.");
            }

            _systems = _systems.OrderBy(i => order[i.SystemType]).ToList();

            foreach (var system in _systems)
            {
                system.Initialize();
            }
        }
        
        public void Update()
        {
            foreach (var system in _systems)
            {
                system.Update();
            }
        }

        public void FinalizeObject()
        {
            foreach (var system in _systems)
            {
                system.FinalizeSystem();
            }
        }
    }
}