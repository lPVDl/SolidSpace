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
        private readonly GameCycleConfig _config;
        private readonly IProfilingManager _profilingManager;
        private readonly IProfilingProcessor _profilingProcessor;

        private readonly List<IInitializable> _initializables;
        private readonly List<IUpdatable> _updatables;
        
        private string[] _gameCycleNames;
        private IUpdatable[] _gameCycle;
        private IInitializable[] _initializationSequence;
        private UpdatingBehaviour _updateBehaviour;
        private ProfilingHandle _profilingHandle;

        public GameCycleController(List<IInitializable> initializables, List<IUpdatable> updatables, GameCycleConfig config,
            IProfilingManager profilingManager, IProfilingProcessor profilingProcessor)
        {
            _initializables = initializables;
            _updatables = updatables;
            _config = config;
            _profilingManager = profilingManager;
            _profilingProcessor = profilingProcessor;
        }
        
        public void Run()
        {
            _profilingProcessor.Initialize();
            _profilingHandle = _profilingManager.GetHandle(this);
            _gameCycle = PrepareSequence(_updatables, _config.UpdateOrder.Groups, "update");
            _initializationSequence = PrepareSequence(_initializables, _config.InitializationOrder.Groups, "initialization");

            foreach (var item in _initializationSequence)
            {
                item.Initialize();
            }

            _gameCycleNames = _gameCycle.Select(i => i.GetType().Name).ToArray();
            
            var gameObject = new GameObject(nameof(GameCycleController));
            _updateBehaviour = gameObject.AddComponent<UpdatingBehaviour>();
            _updateBehaviour.OnUpdate += OnUpdate;
        }

        private T[] PrepareSequence<T>(ICollection<T> instances, IReadOnlyCollection<ControllerGroup> order, string orderName)
        {
            var intOrder = new Dictionary<Type, int>(); 
            var j = 0;
            foreach (var typeName in order.SelectMany(g => g.Controllers))
            {
                var type = Type.GetType(typeName);
                if (type is null)
                {
                    throw new InvalidOperationException($"Can not resolve type for '{typeName}'");
                }
                
                intOrder.Add(type, j++);
            }
            
            var sequence = new T[instances.Count];
            foreach (var instance in instances)
            {
                var type = instance.GetType();
                if (!intOrder.TryGetValue(type, out var index))
                {
                    throw new InvalidOperationException($"Type '{type.FullName}' is not defined in {orderName} order");
                }

                sequence[index] = instance;
            }

            return sequence;
        }

        private void OnUpdate()
        {
            for (var i = 0; i < _gameCycle.Length; i++)
            {
                _profilingHandle.BeginSample(_gameCycleNames[i]);
                _gameCycle[i].Update();
                _profilingHandle.EndSample(_gameCycleNames[i]);
            }

            _profilingProcessor.Update();
        }

        public void Dispose()
        {
            _updateBehaviour.OnUpdate -= OnUpdate;

            foreach (var controller in _initializables)
            {
                controller.Finalize();
            }
            
            _profilingProcessor.FinalizeObject();
        }
    }
}