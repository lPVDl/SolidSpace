using System.Collections.Generic;
using SolidSpace.GameCycle;

namespace SolidSpace.Playground.Core
{
    public class PlaygroundToolValueStorage : IPlaygroundToolValueStorage, IInitializable
    {
        private Dictionary<string, float> _storage;
        
        public void OnInitialize()
        {
            _storage = new Dictionary<string, float>();
        }
        
        public float GetValueOrDefault(string name)
        {
            _storage.TryGetValue(name, out var value);
            
            return value;
        }

        public void SetValue(string name, float value)
        {
            _storage[name] = value;
        }

        public void OnFinalize()
        {
            
        }
    }
}