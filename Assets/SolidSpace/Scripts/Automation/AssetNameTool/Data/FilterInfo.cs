using System;

namespace SolidSpace.Automation.AssetNameTool
{
    [Serializable]
    internal struct FilterInfo
    {
        public bool enabled;
        public string scannerRegex;
        public string nameRegex;
        public string nameSubstitution;
    }
}