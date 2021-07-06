using System;
using SolidSpace.RegularExpressions;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    [Serializable]
    public class GizmosConfig
    {
        public Shader Shader => _shader;
        public int WindowItemCount => _windowItemCount;
        public int WindowScrollDelta => _windowScrollDelta;
        public RegexPatternSubstitution HandleNameConverter => _handleNameConverter;

        [SerializeField] private Shader _shader;
        [SerializeField] private int _windowItemCount;
        [SerializeField] private int _windowScrollDelta;
        [SerializeField] private RegexPatternSubstitution _handleNameConverter;
    }
}