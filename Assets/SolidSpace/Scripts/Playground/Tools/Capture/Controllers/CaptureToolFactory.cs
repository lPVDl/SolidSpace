using System.Collections.Generic;
using SolidSpace.Entities.World;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.ComponentFilter;
using SolidSpace.Playground.Tools.EntitySearch;
using SolidSpace.Playground.Tools.Spawn;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.Capture
{
    internal class CaptureToolFactory : ICaptureToolFactory
    {
        private readonly IComponentFilterFactory _filterFactory;
        private readonly IEntitySearchSystem _searchSystem;
        private readonly IPlaygroundUIManager _playgroundUI;
        private readonly IUIManager _uiManager;
        private readonly IPointerTracker _pointer;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly IEntityWorldManager _entityManager;
        private readonly IPlaygroundToolValueStorage _valueStorage;

        public CaptureToolFactory(IComponentFilterFactory filterFactory, IEntitySearchSystem searchSystem, 
            IPlaygroundUIManager playgroundUI, IUIManager uiManager, IPointerTracker pointer, IPlaygroundUIFactory uiFactory, 
            IEntityWorldManager entityManager, IPlaygroundToolValueStorage valueStorage)
        {
            _filterFactory = filterFactory;
            _searchSystem = searchSystem;
            _playgroundUI = playgroundUI;
            _uiManager = uiManager;
            _pointer = pointer;
            _uiFactory = uiFactory;
            _entityManager = entityManager;
            _valueStorage = valueStorage;
        }
        
        public ICaptureTool Create(ICaptureToolHandler handler, params ComponentType[] requiredComponents)
        {
            var window = _uiFactory.CreateToolWindow();
            window.SetTitle("Capture");

            var searchRadiusField = _uiFactory.CreateStringField();
            searchRadiusField.SetLabel("Radius");
            searchRadiusField.SetValue("0");
            searchRadiusField.SetValueCorrectionBehaviour(new IntMaxBehaviour(0));
            window.AttachChild(searchRadiusField);
            
            var tool = new CaptureTool
            {
                SearchSystem = _searchSystem,
                Filter = _filterFactory.Create(requiredComponents),
                PlaygroundUI = _playgroundUI,
                UIManager = _uiManager,
                Pointer = _pointer,
                CapturedEntities = new List<Entity>(),
                CapturedPositions = new List<float2>(),
                SearchRadius = 0,
                SearchRadiusField = searchRadiusField,
                Handler = handler,
                CapturedPointer = float2.zero,
                EntityManager = _entityManager,
                ValueStorage = _valueStorage,
                Window = window
            };

            tool.Filter.FilterModified += tool.UpdateSearchSystemQuery;
            tool.SearchRadiusField.ValueChanged += tool.OnSearchRadiusFieldChange;

            return tool;
        }
    }
}