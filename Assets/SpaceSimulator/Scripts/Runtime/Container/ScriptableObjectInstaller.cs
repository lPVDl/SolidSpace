using UnityEngine;
using Zenject;

namespace SpaceSimulator.Runtime
{
    public abstract class ScriptableObjectInstaller : ScriptableObject
    {
        public abstract void InstallBindings(DiContainer container);
    }
}