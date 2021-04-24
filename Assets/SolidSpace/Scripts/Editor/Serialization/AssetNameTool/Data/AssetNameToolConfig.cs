using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.Editor.Serialization.AssetNameTool
{
    [System.Serializable]
    public class AssetNameToolConfig
    {
        public IReadOnlyList<AssetNameToolFolderFilter> Folders => _folders;
        
        [SerializeField] private List<AssetNameToolFolderFilter> _folders;
    }
}