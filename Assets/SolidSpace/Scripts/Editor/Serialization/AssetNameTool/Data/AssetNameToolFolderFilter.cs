using System;

namespace SolidSpace.Editor.Serialization.AssetNameTool
{
    [Serializable]
    public struct AssetNameToolFolderFilter
    {
        public bool enabled;
        public string scannerRegex;
        public string nameRegex;
        public string nameSubstitution;
    }
}