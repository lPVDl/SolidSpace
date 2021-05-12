using System;
using System.Collections.Generic;
using System.Linq;
using SolidSpace.GameCycle;
using SolidSpace.Profiling;
using UnityEngine;

namespace SolidSpace.Entities
{
    public class EntityCycleController : IController
    {
        public EControllerType ControllerType => EControllerType.Common;

        private readonly EntityCycleConfig _config;
        private readonly IProfilingManager _profilingManager;

        private List<IEntitySystem> _systems;
        private List<string> _names;
        private ProfilingHandle _profiler;

        public EntityCycleController(List<IEntitySystem> systems, EntityCycleConfig config, IProfilingManager profilingManager)
        {
            _systems = systems;
            _config = config;
            _profilingManager = profilingManager;
        }
        
        public void Initialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            
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
            _names = _systems.Select(i => i.GetType().Name).ToList();

            foreach (var system in _systems)
            {
                system.Initialize();
            }
        }
        
        public void Update()
        {
            for (var i = 0; i < _systems.Count; i++)
            {
                _profiler.BeginSample(_names[i]);
                _systems[i].Update();
                _profiler.EndSample(_names[i]);
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