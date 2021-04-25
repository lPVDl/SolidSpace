using System;
using System.Collections.Generic;
using System.Linq;
using SolidSpace.Profiling;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace SolidSpace
{
    public partial class GameCycleController : Zenject.IInitializable, IDisposable
    {
        private readonly GameCycleConfig _config;
        private readonly IProfilingManager _profilingManager;

        private List<IController> _controllers;
        private List<string> _names;

        private UpdateHandler _updateHandler;
        private ProfilingHandle _profiler;

        public GameCycleController(List<IController> controllers, GameCycleConfig config, IProfilingManager profilingManager)
        {
            _controllers = controllers;
            _config = config;
            _profilingManager = profilingManager;
        }
        
        public void Initialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            
            var order = new Dictionary<EControllerType, int>();
            for (var i = 0; i < _config.InvocationOrder.Count; i++)
            {
                order[_config.InvocationOrder[i]] = i;
            }

            var unordered = _controllers.Where(i => !order.ContainsKey(i.ControllerType)).ToList();
            
            if (unordered.Any())
            {
                foreach (var controller in unordered)
                {
                    var message = $"{controller.GetType()} ({controller.ControllerType}) is missing in update order list.";
                    Debug.LogError(message);
                }
                
                throw new InvalidOperationException("Failed to create execution order.");
            }

            _controllers = _controllers.OrderBy(i => order[i.ControllerType]).ToList();
            _names = _controllers.Select(i => i.GetType().Name).ToList();

            if (_controllers.Count == 0 || _controllers[0].ControllerType != EControllerType.Profiling)
            {
                throw new InvalidOperationException("Profiling controller was not recognized.");
            }

            foreach (var controller in _controllers)
            {
                controller.Initialize();
            }

            var gameObject = new GameObject("GameCycleController");
            _updateHandler = gameObject.AddComponent<UpdateHandler>();
            _updateHandler.OnUpdate += OnUpdate;
        }

        private void OnUpdate()
        {
            _controllers[0].Update();
            
            for (var i = 1; i < _controllers.Count; i++)
            {
                _profiler.BeginSample(_names[i]);
                _controllers[i].Update();
                _profiler.EndSample();
            }
        }

        public void Dispose()
        {
            _updateHandler.OnUpdate -= OnUpdate;

            foreach (var controller in _controllers)
            {
                controller.FinalizeObject();
            }
        }
    }
}