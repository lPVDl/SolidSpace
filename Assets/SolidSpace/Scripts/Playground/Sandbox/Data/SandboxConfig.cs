using System;
using SolidSpace.Playground.UI;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox
{
    [Serializable]
    internal class SandboxConfig
    {
        public UIPrefab<ICheckedButtonView> CheckedButtonPrefab => _checkedButtonPrefab;

        [SerializeField] private UIPrefab<ICheckedButtonView> _checkedButtonPrefab;
    }
}