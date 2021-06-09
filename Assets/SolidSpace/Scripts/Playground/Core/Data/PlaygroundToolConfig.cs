using System;
using UnityEngine;

namespace SolidSpace.Playground.Core
{
    [Serializable]
    public class PlaygroundToolConfig
    {
        public Sprite Icon => _icon;

        [SerializeField] private Sprite _icon;
    }
}