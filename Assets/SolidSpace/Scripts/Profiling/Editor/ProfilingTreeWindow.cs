using System;
using System.Collections.Generic;
using System.Diagnostics;
using SolidSpace.DebugUtils;
using SolidSpace.Profiling.Controllers;
using SolidSpace.Profiling.Data;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.Profiling.Editor
{
    public class ProfilingTreeWindow : EditorWindow
    {
        private Stopwatch _stopwatch;
        private List<ProfilingNodeFriendly> _nodes;

        private int _offset;
        private int _yScroll;

        private void OnGUI()
        {
            var profilingManager = ProfilingManager.Instance;
            if (profilingManager is null)
            {
                return;
            }
            
            var currentEvent = Event.current;
            if (currentEvent.isScrollWheel)
            {
                _yScroll = (int) currentEvent.delta.y * 10;
                return;
            }

            _stopwatch ??= new Stopwatch();
            _stopwatch.Reset();
            _stopwatch.Start();
            
            DrawTreeJobSafe(profilingManager.Reader);
            
            _stopwatch.Stop();
            
            SpaceDebug.LogState("DrawTree ms", _stopwatch.ElapsedTicks / (float) Stopwatch.Frequency * 1000);
            
            Repaint();
        }

        private void DrawTreeJobSafe(ProfilingTreeReader tree)
        {
            var currentEvent = Event.current;

            if (currentEvent.type == EventType.Layout)
            {
                _nodes ??= new List<ProfilingNodeFriendly>();
                var displayCount = (int) Math.Ceiling(position.height / 20);
                tree.Read(_offset, displayCount, _nodes, out var totalNodeCount);

                var lastNode = Math.Max(0, totalNodeCount - displayCount);
                _offset = Mathf.Clamp(_offset + _yScroll, 0, lastNode);
                _yScroll = 0;
            }

            for (var i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                
                EditorGUI.indentLevel = node.deep;
                EditorGUILayout.LabelField(node.name);
            }
        }
    }
}