using System;
using System.Linq;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.Gizmos;
using SolidSpace.Playground.Core;
using SolidSpace.Playground.Tools.ComponentFilter;
using SolidSpace.Playground.Tools.EntitySearch;
using SolidSpace.Playground.Tools.SpawnPoint;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using Unity.Entities;
using UnityEngine;

namespace SolidSpace.Playground.Tools.Eraser
{
    internal class EraserTool : IPlaygroundTool
    {
        public PlaygroundToolConfig Config { get; private set; }
        
        private readonly IEntityWorldManager _entityManager;
        private readonly IEntitySearchSystem _searchSystem;
        private readonly IPointerTracker _pointer;
        private readonly IPlaygroundUIManager _playgroundUIManager;
        private readonly IComponentFilterFactory _filterFactory;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly IUIManager _uiManager;
        private readonly IPlaygroundToolValueStorage _valueStorage;
        private readonly IGizmosManager _gizmosManager;
        private readonly EraserToolConfig _config;
        
        private GizmosHandle _gizmos;
        private IToolWindow _window;
        private IComponentFilter _filter;
        private IStringField _radiusField;
        private int _radius;

        public EraserTool(EraserToolConfig config, IEntityWorldManager entityManager, IGizmosManager gizmosManager,
            IEntitySearchSystem searchSystem, IPointerTracker pointer, IPlaygroundUIManager playgroundUIManager, 
            IComponentFilterFactory filterFactory, IPlaygroundUIFactory uiFactory, IUIManager uiManager,
            IPlaygroundToolValueStorage valueStorage)
        {
            _entityManager = entityManager;
            _searchSystem = searchSystem;
            _pointer = pointer;
            _playgroundUIManager = playgroundUIManager;
            _filterFactory = filterFactory;
            _uiFactory = uiFactory;
            _uiManager = uiManager;
            _valueStorage = valueStorage;
            _gizmosManager = gizmosManager;
            _config = config;
        }
        
        public void OnInitialize()
        {
            Config = _config.ToolConfig;
            _gizmos = _gizmosManager.GetHandle(this);

            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Eraser");

            _filter = _filterFactory.Create(typeof(PositionComponent));
            _filter.FilterModified += UpdateSearchSystemQuery;

            _radiusField = _uiFactory.CreateStringField();
            _radiusField.SetLabel("Radius");
            _radiusField.SetValue("0");
            _radiusField.SetValueCorrectionBehaviour(new IntMaxBehaviour(0));
            _radiusField.ValueChanged += OnRadiusFieldChanged;
            _window.AttachChild(_radiusField);

            var button = _uiFactory.CreateGeneralButton();
            button.SetLabel("Destroy matching");
            button.Clicked += OnDestroyClicked;
            _window.AttachChild(button);
        }
        
        public void OnActivate(bool isActive)
        {
            if (isActive)
            {
                UpdateSearchSystemQuery();
                
                _radius = (int) _valueStorage.GetValueOrDefault("InteractionRange");
                _radius = Math.Max(0, _radius);
                _radiusField.SetValue(_radius.ToString());
                _searchSystem.SetSearchRadius(_radius);
            }
            else
            {
                _valueStorage.SetValue("InteractionRange", _radius);
            }
            
            _searchSystem.SetEnabled(isActive);
            _playgroundUIManager.SetElementVisible(_filter, isActive);
            _playgroundUIManager.SetElementVisible(_window, isActive);
        }

        public void OnUpdate()
        {
            if (_uiManager.IsMouseOver)
            {
                return;
            }
            
            _searchSystem.SetSearchPosition(_pointer.Position);

            var result = _searchSystem.Result;
            
            if (_radius == 0)
            {
                if (!result.isValid)
                {
                    return;
                }
                
                _gizmos.DrawLine(_pointer.Position, result.nearestPosition, Color.red);
                _gizmos.DrawScreenSquare(result.nearestPosition, 6, Color.red);
                _gizmos.DrawScreenSquare(result.nearestPosition, 4, Color.black);
                
                if (_pointer.ClickedThisFrame)
                {
                    _entityManager.DestroyEntity(result.nearestEntity);
                }
            }
            else
            {
                _gizmos.DrawWirePolygon(_pointer.Position, _radius, 64, Color.red);

                if (!result.isValid)
                {
                    return;
                }

                for (var i = 0; i < result.inRadiusCount; i++)
                {
                    var position = result.inRadiusPositions[i];
                    _gizmos.DrawScreenSquare(position, 6, Color.red);
                    _gizmos.DrawScreenSquare(position, 4, Color.black);
                }

                if (_pointer.ClickedThisFrame)
                {
                    _entityManager.DestroyEntity(result.inRadiusEntities);
                }
            }
        }

        private void OnRadiusFieldChanged()
        {
            _radius = int.Parse(_radiusField.Value);
            _searchSystem.SetSearchRadius(_radius);
        }

        private void UpdateSearchSystemQuery()
        {
            _searchSystem.SetQuery(new EntityQueryDesc
            {
                All = _filter.GetTagsWithState(ETagLabelState.Positive).ToArray(),
                None = _filter.GetTagsWithState(ETagLabelState.Negative).ToArray()
            });
        }

        private void OnDestroyClicked()
        {
            var query = _entityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = _filter.GetTagsWithState(ETagLabelState.Positive).ToArray(),
                None = _filter.GetTagsWithState(ETagLabelState.Negative).ToArray()
            });
            
            _entityManager.DestroyEntity(query);
        }

        public void OnFinalize()
        {
            
        }
    }
}