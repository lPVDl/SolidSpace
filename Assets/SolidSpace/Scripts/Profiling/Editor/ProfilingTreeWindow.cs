using System.Diagnostics;
using SolidSpace.Profiling.Controllers;
using SolidSpace.Profiling.Data;
using UnityEditor;

namespace SolidSpace.Profiling.Editor
{
    public class ProfilingTreeWindow : EditorWindow
    {
        private ProfilingHandle _handle;
        private bool _handleValid;
        private ProfilingTreeView _view;
        private Stopwatch _stopwatch;

        private void OnGUI()
        {
            var profilingManager = ProfilingManager.Instance;
            if (profilingManager is null)
            {
                _handleValid = false;
                return;
            }

            if (!_handleValid)
            {
                _handle = profilingManager.GetHandle(this);
            }

            _stopwatch ??= new Stopwatch();
            
            ModifyTree(_handle);

            var timeSeconds = _stopwatch.ElapsedTicks / (double) Stopwatch.Frequency;
            EditorGUILayout.FloatField("Modification Time (ms)", (float) (timeSeconds * 1000));
            
            _view ??= new ProfilingTreeView();
            // _view.OnGUI(profilingManager.Result);
        }
        
        private void ModifyTree(ProfilingHandle handle)
        {
            _stopwatch.Reset();
            _stopwatch.Start();
            
            for (var i = 0; i < 1 << 16; i++)
            {
                handle.BeginSample("Test");
                handle.EndSample("Test");
            }

            _stopwatch.Stop();
        }

        // private ProfilingTree _tree;
        // private ProfilingTreeView _view;
        //
        // private void OnGUI()
        // {
        //     UpdateTree();
        //
        //     _view ??= new ProfilingTreeView();
        //     _view.OnGUI(_tree);
        // }
        //
        // private void UpdateTree()
        // {
        //     _tree ??= new ProfilingTree();
        //     
        //     _tree.Clear();
        //     
        //     _tree.BeginSample("GameCycle");
        //     {
        //         _tree.BeginSample("EntityController");
        //         
        //         _tree.EndSample();
        //         
        //         _tree.BeginSample("DrawingController");
        //         
        //         _tree.EndSample();
        //     }
        //     _tree.EndSample();
        //     
        //     _tree.BeginSample("MotorCycle");
        //     
        //     _tree.EndSample();
        // }
    }
}