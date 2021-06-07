using System;
using SolidSpace.Playground.UI;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox.Data
{
    [Serializable]
    internal class SandboxConfig
    {
        public UIPrefab UIPrefab => _uiPrefab;
        
        [SerializeField] private UIPrefab _uiPrefab;
    }
}