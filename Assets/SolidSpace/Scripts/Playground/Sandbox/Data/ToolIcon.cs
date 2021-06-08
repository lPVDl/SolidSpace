using System;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox
{
    [Serializable]
    internal struct ToolIcon
    {
        [SerializeField] public EPlaygroundTool tool;
        [SerializeField] public Sprite icon;
    }
}