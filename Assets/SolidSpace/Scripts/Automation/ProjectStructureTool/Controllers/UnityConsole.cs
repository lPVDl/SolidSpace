using SolidSpace.Automation.AssetNameTool;
using UnityEngine;

namespace SolidSpace.Automation.ProjectStructureTool
{
    internal class UnityConsole : IConsole
    {
        public UnityConsole(bool clearConsole)
        {
            if (clearConsole)
            {
                ConsoleUtil.ClearLog();
            }
        }
        
        public void WriteLine(string text)
        {
            Debug.Log(text);
        }
    }
}