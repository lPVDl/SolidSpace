using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.Capture;
using SolidSpace.Playground.UI;
using UnityEngine;

namespace SolidSpace.Playground.Tools.Eraser
{
    internal class EraserTool : IPlaygroundTool, ICaptureToolHandler
    {
        public PlaygroundToolConfig Config { get; private set; }
        
        private readonly IEntityWorldManager _entityManager;
        private readonly IPlaygroundUIManager _playgroundUIManager;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly ICaptureToolFactory _captureToolFactory;
        private readonly EraserToolConfig _config;
        
        private ICaptureTool _captureTool;
        private IToolWindow _window;

        public EraserTool(EraserToolConfig config, IEntityWorldManager entityManager, IPlaygroundUIManager playgroundUIManager, 
            IPlaygroundUIFactory uiFactory, ICaptureToolFactory captureToolFactory)
        {
            _entityManager = entityManager;
            _playgroundUIManager = playgroundUIManager;
            _uiFactory = uiFactory;
            _captureToolFactory = captureToolFactory;
            _config = config;
        }
        
        public void OnInitialize()
        {
            Config = _config.ToolConfig;

            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Eraser");

            var button = _uiFactory.CreateGeneralButton();
            button.SetLabel("Destroy all");
            button.Clicked += OnDestroyClicked;
            _window.AttachChild(button);

            _captureTool = _captureToolFactory.Create(this, Color.red, typeof(PositionComponent));
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
            _entityManager.DestroyEntity(eventData.entity);
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