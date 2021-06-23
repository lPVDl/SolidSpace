using System;
using SolidSpace.Entities.Actors.Interfaces;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.Capture;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ActorControl
{
    public class ActorControlTool : IPlaygroundTool, ICaptureToolHandler
    {
        private readonly ICaptureToolFactory _captureToolFactory;
        private readonly IGizmosManager _gizmosManager;
        private readonly IActorControlSystem _actorControlSystem;
        private readonly IPointerTracker _pointer;
        private readonly IEntityWorldManager _entityManager;

        private GizmosHandle _gizmos;
        private ICaptureTool _captureTool;

        public ActorControlTool(ICaptureToolFactory captureToolFactory, IGizmosManager gizmosManager, 
            IActorControlSystem actorControlSystem, IPointerTracker pointer, IEntityWorldManager entityManager)
        {
            _captureToolFactory = captureToolFactory;
            _gizmosManager = gizmosManager;
            _actorControlSystem = actorControlSystem;
            _pointer = pointer;
            _entityManager = entityManager;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this, Color.magenta);
            _captureTool = _captureToolFactory.Create(this, typeof(PositionComponent), typeof(ActorComponent));
        }

        public void OnUpdate()
        {
            _gizmos.DrawWireRect(_pointer.Position, new float2(8, 8), (float) (Math.PI * 0.25));
            _actorControlSystem.SetActorsTargetPosition(_pointer.Position);
            _captureTool.OnUpdate();
        }

        public void OnActivate(bool isActive)
        {
            _captureTool.OnActivate(isActive);
        }
        
        public void OnCaptureEvent(CaptureEventData eventData)
        {
            switch (eventData.eventType)
            {
                case ECaptureEventType.CaptureStart:
                    var actorData = _entityManager.GetComponentData<ActorComponent>(eventData.entity);
                    actorData.isActive = !actorData.isActive;
                    _entityManager.SetComponentData(eventData.entity, actorData);
                    break;
                
                case ECaptureEventType.CaptureUpdate:
                    break;
                
                case ECaptureEventType.CaptureEnd:
                    break;
                
                case ECaptureEventType.SelectionSingle:
                    _gizmos.DrawScreenSquare(eventData.entityPosition, 6);
                    _gizmos.DrawLine(eventData.currentPointer, eventData.entityPosition);
                    break;
                
                case ECaptureEventType.SelectionMultiple:
                    _gizmos.DrawScreenSquare(eventData.entityPosition, 6);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnDrawSelectionCircle(float2 position, float radius)
        {
            _gizmos.DrawWirePolygon(position, radius, 48);
        }

        public void OnFinalize() { }
    }
}