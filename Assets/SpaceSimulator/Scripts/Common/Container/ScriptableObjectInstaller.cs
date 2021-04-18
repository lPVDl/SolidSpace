using UnityEngine;

namespace SpaceSimulator
{
    public abstract class ScriptableObjectInstaller : ScriptableObject
    {
        public abstract void InstallBindings(IContainer container);
    }
}