using System;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    [Serializable]
    public class ComponentFilterFactoryConfig
    {
        public string NameRegex => _nameRegex;
        public string NameSubstitution => _nameSubstitution;
        public string FilterRegex => _filterRegex;

        [SerializeField] private string _filterRegex;
        [SerializeField] private string _nameRegex;
        [SerializeField] private string _nameSubstitution;
    }
}