using UnityEngine;

namespace SpaceSimulator.Runtime
{
    public abstract class ScriptableObjectInstaller : ScriptableObject
    {
        public abstract void InstallBindings(IContainer container);
    }
}