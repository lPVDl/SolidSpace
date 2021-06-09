using System;
using UnityEngine;

namespace SolidSpace.Playground.Sandbox
{
    [Serializable]
    public class PlaygroundToolConfig
    {
        public Sprite Icon => _icon;

        [SerializeField] private Sprite _icon;
    }
}