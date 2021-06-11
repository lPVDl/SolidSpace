using System;
using System.Text.RegularExpressions;
using SolidSpace.GameCycle;
using SolidSpace.Playground.UI;
using SolidSpace.UI;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public class ComponentFilterWindow : IController, IComponentFilterTool
    {
        public event Action FilterModified;
        
        public EControllerType ControllerType => EControllerType.UI;
        
        private readonly ComponentFilterWindowConfig _config;
        private readonly IUIManager _uiManager;
        private readonly IPlaygroundUIFactory _uiFactory;
        private readonly IComponentFilterMaster _filterMaster;

        private IToolWindow _window;
        private FilterState[] _filterStates;
        private ITagLabel[] _filterViews;

        public ComponentFilterWindow(ComponentFilterWindowConfig config, IUIManager uiManager, 
            IPlaygroundUIFactory uiFactory, IComponentFilterMaster filterMaster)
        {
            _config = config;
            _uiManager = uiManager;
            _uiFactory = uiFactory;
            _filterMaster = filterMaster;
        }
        
        public void InitializeController()
        {
            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Component Filter");

            var container = _uiFactory.CreateLayoutGrid();
            container.SetFlexDirection(FlexDirection.Row);
            container.SetFlexWrap(Wrap.Wrap);
            _window.AttachChild(container);

            var components = _filterMaster.AllComponents;
            _filterStates = _filterMaster.CreateFilter();
            _filterViews = new ITagLabel[components.Count];
            for (var i = 0; i < _filterStates.Length; i++)
            {
                var type = components[i];
                var state = _filterStates[i];
                var view = _uiFactory.CreateTagLabel();
                var labelIndex = i;
                var name = Regex.Replace(type.ToString(), _config.ComponentNameRegex, _config.ComponentNameSubstitution);
                view.SetLabel(name);
                view.SetState(state.state);
                view.SetLocked(state.isLocked);
                view.Clicked += () => OnLabelClicked(labelIndex);
                container.AttachChild(view);
                _filterViews[i] = view;
            }
        }

        private void OnLabelClicked(int index)
        {
            var filterState = _filterStates[index];
            if (filterState.isLocked)
            {
                return;
            }

            filterState.state = AdvanceState(filterState.state);
            _filterStates[index] = filterState;
            _filterViews[index].SetState(filterState.state);
            
            FilterModified?.Invoke();
        }
        
        private ETagLabelState AdvanceState(ETagLabelState state)
        {
            return state switch
            {
                ETagLabelState.Neutral => ETagLabelState.Positive,
                ETagLabelState.Positive => ETagLabelState.Negative,
                ETagLabelState.Negative => ETagLabelState.Neutral,
                
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        public void UpdateController() { }

        public void SetEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                _uiManager.AddToRoot(_window, "ContainerA");
            }
            else
            {
                _uiManager.RemoveFromRoot(_window, "ContainerA");
            }
        }
        
        public void SetFilter(FilterState[] newFilter)
        {
            for (var i = 0; i < newFilter.Length; i++)
            {
                var newState = newFilter[i];
                var oldState = _filterStates[i];

                if ((newState.isLocked != oldState.isLocked) || (newState.state != oldState.state))
                {
                    _filterStates[i] = newState;
                    var view = _filterViews[i];
                    view.SetLocked(newState.isLocked);
                    view.SetState(newState.state);
                }
            }
        }
        
        public void GetFilter(FilterState[] output)
        {
            Array.Copy(_filterStates, output, _filterStates.Length);
        }

        public void FinalizeController() { }
    }
}