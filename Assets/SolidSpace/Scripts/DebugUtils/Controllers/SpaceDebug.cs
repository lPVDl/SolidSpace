using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.DebugUtils
{
    public static class SpaceDebug
    {
        public static IReadOnlyDictionary<string, SpaceDebugValue<int>> IntStates => _intStates;
        public static IReadOnlyDictionary<string, SpaceDebugValue<float>> FloatStates => _floatStates;

        private static readonly Dictionary<string, SpaceDebugValue<int>> _intStates;
        private static readonly Dictionary<string, SpaceDebugValue<float>> _floatStates;
        
        static SpaceDebug()
        {
            _intStates = new Dictionary<string, SpaceDebugValue<int>>();
            _floatStates = new Dictionary<string, SpaceDebugValue<float>>();
        }
        
        public static void LogState(string id, int value)
        {
            _intStates[id] = new SpaceDebugValue<int>
            {
                value = value,
                logTime = Time.time
            };
        }
        
        public static void LogState(string id, float value)
        {
            _floatStates[id] = new SpaceDebugValue<float>
            {
                value = value,
                logTime = Time.time
            };
        }
    }
}