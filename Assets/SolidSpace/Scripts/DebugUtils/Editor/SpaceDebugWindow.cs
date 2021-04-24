using UnityEditor;
using UnityEngine;

namespace SolidSpace.DebugUtils.Editor
{
    public class SpaceDebugWindow : EditorWindow
    {
        [MenuItem("Window/Analysis/Space Debug")]
        private static void OpenWindow()
        {
            GetWindow<SpaceDebugWindow>("Space Debug");
        }
        
        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            foreach (var floatState in SpaceDebug.FloatStates)
            {
                EditorGUILayout.FloatField(floatState.Key, floatState.Value.value);
            }

            foreach (var intState in SpaceDebug.IntStates)
            {
                EditorGUILayout.IntField(intState.Key, intState.Value.value);
            }

            Repaint();
        }
    }
}