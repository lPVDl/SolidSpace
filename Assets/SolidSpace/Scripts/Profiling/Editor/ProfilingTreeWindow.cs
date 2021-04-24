using SolidSpace.Profiling.Controllers;
using UnityEditor;

namespace SolidSpace.Profiling.Editor
{
    public class ProfilingTreeWindow : EditorWindow
    {
        private ProfilingTree _tree;
        private ProfilingTreeView _view;
        
        private void OnGUI()
        {
            UpdateTree();

            _view ??= new ProfilingTreeView();
            _view.OnGUI(_tree);
        }

        private void UpdateTree()
        {
            _tree ??= new ProfilingTree();
            
            _tree.Clear();
            
            _tree.BeginSample("GameCycle");
            {
                _tree.BeginSample("EntityController");
                
                _tree.EndSample();
                
                _tree.BeginSample("DrawingController");
                
                _tree.EndSample();
            }
            _tree.EndSample();
            
            _tree.BeginSample("MotorCycle");
            
            _tree.EndSample();
        }
    }
}