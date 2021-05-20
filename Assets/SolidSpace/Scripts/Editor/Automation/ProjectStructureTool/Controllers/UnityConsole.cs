using SolidSpace.Editor.Utilities;
using UnityEngine;

namespace SolidSpace.Editor.Automation.ProjectStructureTool
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