using System;
using System.Collections.Generic;
using System.Linq;
using SolidSpace.DependencyInjection;
using SolidSpace.Profiling;
using UnityEngine;

namespace SolidSpace.GameCycle
{
    internal class GameCycleController : IApplicationBootstrapper, IDisposable
    {
        private readonly Config _config;
        private readonly IProfilingManager _profilingManager;
        private readonly IProfilingProcessor _profilingProcessor;

        private List<IController> _controllers;
        private List<string> _names;

        private UpdatingBehaviour _behaviour;
        private ProfilingHandle _profilingHandle;

        public GameCycleController(List<IController> controllers, Config config,
            IProfilingManager profilingManager, IProfilingProcessor profilingProcessor)
        {
            _controllers = controllers;
            _config = config;
            _profilingManager = profilingManager;
            _profilingProcessor = profilingProcessor;
        }
        
        public void Run()
        {
            _profilingProcessor.Initialize();
            
            _profilingHandle = _profilingManager.GetHandle(this);
            
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
                    var message = $"{controller.GetType()} ({controller.ControllerType}) is missing in the execution order list.";
                    throw new InvalidOperationException(message);
                }
            }

            // TODO [T-18]: Separate initialization and execution order in GameCycle.
            _controllers = _controllers.OrderBy(i => order[i.ControllerType]).ToList();
            _names = _controllers.Select(i => i.GetType().Name).ToList();

            foreach (var controller in _controllers)
            {
                controller.InitializeController();
            }

            var gameObject = new GameObject(nameof(GameCycleController));
            _behaviour = gameObject.AddComponent<UpdatingBehaviour>();
            _behaviour.OnUpdate += OnUpdate;
        }

        private void OnUpdate()
        {
            for (var i = 0; i < _controllers.Count; i++)
            {
                _profilingHandle.BeginSample(_names[i]);
                _controllers[i].UpdateController();
                _profilingHandle.EndSample(_names[i]);
            }

            _profilingProcessor.Update();
        }

        public void Dispose()
        {
            _behaviour.OnUpdate -= OnUpdate;

            foreach (var controller in _controllers)
            {
                controller.FinalizeController();
            }
            
            _profilingProcessor.FinalizeObject();
        }
    }
}