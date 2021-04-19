using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceSimulator
{
    public partial class UpdatingController : IInitializable, IFinalazable
    {
        public EControllerType ControllerType => EControllerType.Common;
        
        private readonly GameCycleConfig _config;
        
        private List<IUpdatable> _updatables;

        private UpdateHandler _updateHandler;

        public UpdatingController(List<IUpdatable> updatables, GameCycleConfig config)
        {
            _updatables = updatables;
            _config = config;
        }
        
        public void Initialize()
        {
            var order = new Dictionary<EControllerType, int>();
            for (var i = 0; i < _config.InvocationOrder.Count; i++)
            {
                order[_config.InvocationOrder[i]] = i;
            }

            var unordered = _updatables.Where(i => !order.ContainsKey(i.ControllerType)).ToList();
            
            if (unordered.Any())
            {
                foreach (var controller in unordered)
                {
                    var message = $"{controller.GetType()} ({controller.ControllerType}) is missing in update order list.";
                    Debug.LogError(message);
                }
                
                throw new InvalidOperationException("Failed to create update order.");
            }

            _updatables = _updatables.OrderBy(i => order[i.ControllerType]).ToList();

            var gameObject = new GameObject("UpdatingController");
            _updateHandler = gameObject.AddComponent<UpdateHandler>();
            _updateHandler.OnUpdate += OnUpdate;
        }

        private void OnUpdate()
        {
            foreach (var item in _updatables)
            {
                item.Update();
            }
        }

        public void FinalizeObject()
        {
            _updateHandler.OnUpdate -= OnUpdate;
        }
    }
}