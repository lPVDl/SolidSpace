using System;
using System.Collections.Generic;
using SolidSpace.GameCycle;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.Profiling
{
    public class ProfilingWindow : IInitializable, IUpdatable
    {
        private class IMGUIContainerWrapper : IUIElement
        {
            public VisualElement Root => Container;
            public IMGUIContainer Container { get; }

            public IMGUIContainerWrapper(int height, Action onGUI)
            {
                Container = new IMGUIContainer(onGUI);
                Container.style.height = new StyleLength(height + 20);
                Container.style.marginTop = new StyleLength(-20);
            }
        }
        
        private readonly IUIFactory _uiFactory;
        private readonly IUIManager _uiManager;
        private readonly ProfilingConfig _config;
        private readonly IProfilingManager _profilingManager;

        private IToolWindow _window;
        private IVerticalFixedItemList _itemList;
        private int _offset;
        private IMGUIContainerWrapper _treeContainer;
        private ProfilingNodesView _nodesView;
        private List<ProfilingNode> _nodes;
        private int _displayNodeCount;
        private int _yScroll;
        
        public ProfilingWindow(IUIFactory uiFactory, IUIManager uiManager, ProfilingConfig config, 
            IProfilingManager profilingManager)
        {
            _uiFactory = uiFactory;
            _uiManager = uiManager;
            _config = config;
            _profilingManager = profilingManager;
        }
        
        public void OnInitialize()
        {
            _displayNodeCount = _config.WindowItemCount;
            _nodes = new List<ProfilingNode>();
            _nodesView = new ProfilingNodesView();
            
            _window = _uiFactory.CreateToolWindow();
            _window.SetTitle("Profiling Tree");
            _uiManager.AttachToRoot(_window, "ContainerB");

            _itemList = _uiFactory.CreateVerticalList();
            _itemList.Scrolled += OnListScroll;
            _window.AttachChild(_itemList);

            _treeContainer = new IMGUIContainerWrapper(_displayNodeCount * 20, OnTreeContainerGUI);
            _itemList.AttachItem(_treeContainer);
        }

        private void OnListScroll(int delta)
        {
            _yScroll += delta * _config.WindowScrollMultiplier;
        }

        private void OnTreeContainerGUI()
        {
            var rect = _treeContainer.Container.contentRect;
            rect.width -= 5;
            GUI.color = Color.black;

            _nodesView.OnGUI(rect, _nodes);
        }
        
        public void OnUpdate()
        {
            _profilingManager.Reader.Read(_offset, _displayNodeCount, _nodes, out var totalNodeCount);
            
            var lastNode = Math.Max(0, totalNodeCount - _displayNodeCount);
            _offset = Mathf.Clamp(_offset + _yScroll, 0, lastNode);
            _yScroll = 0;
            
            var sliderMinMax = new int2(0, totalNodeCount);
            var sliderOffset = new int2(_offset, Math.Min(totalNodeCount, _offset + _nodes.Count));
            _itemList.SetSliderState(sliderMinMax, sliderOffset);
        }

        public void OnFinalize()
        {
            
        }
    }
}