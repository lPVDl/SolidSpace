using UnityEngine;

namespace SolidSpace
{
    public abstract class ScriptableObjectInstaller : ScriptableObject
    {
        public abstract void InstallBindings(IContainer container);
    }
}