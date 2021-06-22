using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.Capture;
using SolidSpace.Playground.UI;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Playground.Tools.Eraser
{
    internal class EraserTool : IPlaygroundTool, ICaptureToolHandler
    {
        private readonly IEntityWorldManager _entityManager;
        private readonly IPlaygroundUIManager _playgroundUIManager;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly ICaptureToolFactory _captureToolFactory;
        private readonly IGizmosManager _gizmosManager;

        private ICaptureTool _captureTool;
        private IToolWindow _window;
        private GizmosHandle _gizmos;

        public EraserTool(IEntityWorldManager entityManager, IPlaygroundUIManager playgroundUIManager, 
            IPlaygroundUIFactory uiFactory, ICaptureToolFactory captureToolFactory, IGizmosManager gizmosManager)
        {
            _entityManager = entityManager;
            _playgroundUIManager = playgroundUIManager;
            _uiFactory = uiFactory;
            _captureToolFactory = captureToolFactory;
            _gizmosManager = gizmosManager;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this, Color.red);
            
            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Eraser");

            var button = _uiFactory.CreateGeneralButton();
            button.SetLabel("Destroy all");
            button.Clicked += OnDestroyClicked;
            _window.AttachChild(button);

            _captureTool = _captureToolFactory.Create(this, typeof(PositionComponent));
        }
        
        public void OnActivate(bool isActive)
        {
            _captureTool.OnActivate(isActive);
            _playgroundUIManager.SetElementVisible(_window, isActive);
        }

        public void OnUpdate()
        {
            _captureTool.OnUpdate();
        }
        
        public void OnCaptureEvent(CaptureEventData eventData)
        {
            switch (eventData.eventType)
            {
                case ECaptureEventType.SelectionSingle:
                    _gizmos.DrawScreenSquare(eventData.startEntityPosition, 6);
                    _gizmos.DrawLine(eventData.currentPointer, eventData.startEntityPosition);
                    break;
                
                case ECaptureEventType.SelectionMultiple:
                    _gizmos.DrawScreenSquare(eventData.startEntityPosition, 6);
                    break;
                
                case ECaptureEventType.CaptureStart:
                    _entityManager.DestroyEntity(eventData.entity);
                    break;
                
                case ECaptureEventType.CaptureUpdate:
                    break;
                
                case ECaptureEventType.CaptureEnd:
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnDrawSelectionCircle(float2 position, float radius)
        {
            _gizmos.DrawWirePolygon(position, radius, 48);
        }

        private void OnDestroyClicked()
        {
            var query = _captureTool.CreateQueryFromCurrentFilter();
            _entityManager.DestroyEntity(query);
        }

        public void OnFinalize()
        {
            
        }
    }
}