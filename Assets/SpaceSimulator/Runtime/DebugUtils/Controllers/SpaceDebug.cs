using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SpaceSimulator.Runtime.DebugUtils
{
    public static class SpaceDebug
    {
        public static IReadOnlyDictionary<string, SpaceDebugValue<int>> IntStates => _intStates;

        private static readonly Dictionary<string, SpaceDebugValue<int>> _intStates;
        
        static SpaceDebug()
        {
            _intStates = new Dictionary<string, SpaceDebugValue<int>>();
        }
        
        // [Conditional("DEVELOPMENT_BUILD")]
        public static void LogState(string id, int value)
        {
            _intStates[id] = new SpaceDebugValue<int>
            {
                value = value,
                logTime = Time.time
            };
        }
    }
}