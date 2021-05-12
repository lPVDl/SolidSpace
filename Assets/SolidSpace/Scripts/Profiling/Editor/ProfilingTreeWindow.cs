using System;
using System.Collections.Generic;
using System.Diagnostics;
using SolidSpace.Debugging;
using SolidSpace.Editor;
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
        private string[] _fractionText;
        private string[] _exponentText;

        private void OnGUI()
        {
            var profilingManager = SceneContextUtil.TryResolve<IProfilingManager>();
            if (profilingManager is null)
            {
                return;
            }
            
            var currentEvent = Event.current;
            if (currentEvent.isScrollWheel)
            {
                _yScroll = (int) currentEvent.delta.y;
                return;
            }

            if (_fractionText == null)
            {
                _fractionText = new string[100];
                for (var i = 0; i < 100; i++)
                {
                    _fractionText[i] = "." + i.ToString("D2");
                }
            }

            if (_exponentText == null)
            {
                _exponentText = new string[100];
                for (var i = 0; i < 100; i++)
                {
                    _exponentText[i] = i.ToString("D2");
                }
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
                var displayCount = position.height / 20;
                tree.Read(_offset,  (int) Math.Ceiling(displayCount), _nodes, out var totalNodeCount);

                var lastNode = Math.Max(0, totalNodeCount - (int) Math.Floor(displayCount));
                _offset = Mathf.Clamp(_offset + _yScroll, 0, lastNode);
                _yScroll = 0;
            }

            var labelRect = EditorGUILayout.GetControlRect(false);

            labelRect.height = 20;
            
            var timeRectLeft = new Rect(labelRect.width - 32, labelRect.y, 20, 20);
            var timeRectRight = new Rect(labelRect.width - 19, labelRect.y, 25, 20);
            
            for (var i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];

                labelRect.x = 20 * node.deep + 3;

                GUI.Label(labelRect, node.name);
                
                TimeToString(node.time, out var timeTextLeft, out var timeTextRight);
                if ((int) node.time > 0)
                {
                    GUI.Label(timeRectLeft, timeTextLeft);
                }
                
                GUI.Label(timeRectRight, timeTextRight);

                labelRect.y += 20;
                timeRectRight.y += 20;
                timeRectLeft.y += 20;
            }
        }

        private void TimeToString(float time, out string left, out string right)
        {
            left = _exponentText[Math.Min(99, (int) time)];
            right = _fractionText[(int) (time % 1 * 100)];
        }
    }
}