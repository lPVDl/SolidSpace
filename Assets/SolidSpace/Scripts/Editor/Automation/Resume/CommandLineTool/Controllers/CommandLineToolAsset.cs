using System.Diagnostics;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SolidSpace.Editor.Automation.Resume.CommandLineTool
{
    public class CommandLineToolAsset : ScriptableObject
    {
        [SerializeField] private CommandLineToolConfig _config;

        [Button]
        private void InvokeCommandLines()
        {
            foreach (var command in _config.CommandLines)
            {
                Process.Start("cmd.exe",  $"/c \"{command}\"");
            }
        }
    }
}