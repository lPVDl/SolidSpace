using System;
using UnityEngine;

namespace SolidSpace.Playground.Core
{
    [Serializable]
    public class PlaygroundToolConfig
    {
        public Sprite Icon => _icon;
        public string Name => _name;

        [SerializeField] private string _name;
        [SerializeField] private Sprite _icon;
    }
}