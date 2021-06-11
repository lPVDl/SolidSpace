using UnityEngine;
using System;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    [Serializable]
    public class ComponentFilterMasterConfig
    {
        public string FilterRegex => _filterRegex;

        [SerializeField] private string _filterRegex;
    }
}