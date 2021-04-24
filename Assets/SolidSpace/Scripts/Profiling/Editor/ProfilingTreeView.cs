using SolidSpace.Profiling.Interfaces;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.Profiling.Editor
{
    public class ProfilingTreeView
    {
        public void OnGUI(IProfilingTree tree)
        {
            DrawNodeRecursive(tree, 0, 0);
        }

        private void DrawNodeRecursive(IProfilingTree tree, int nodeIndex, int indent)
        {
            EditorGUI.indentLevel = indent;

            var node = tree.Nodes[nodeIndex];
            
            EditorGUILayout.LabelField(tree.Names[node.name]);

            var siblingIndex = node.child;
            
            while (siblingIndex != 0)
            {
                DrawNodeRecursive(tree, siblingIndex, indent + 1);
                siblingIndex = tree.Nodes[siblingIndex].sibling;
            }
        }
    }
}