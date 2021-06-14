using System;
using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.GameCycle
{
    [Serializable]
    public class ControllerGroup
    {
        public string Name => _name;
        public IReadOnlyList<string> Controllers => _controllers;

        [SerializeField] private string _name;
        [SerializeField] private List<string> _controllers;
    }
}