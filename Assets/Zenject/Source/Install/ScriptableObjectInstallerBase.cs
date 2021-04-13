#if !NOT_UNITY3D

using System;
using UnityEngine;

namespace Zenject
{
    public abstract class ScriptableObjectInstallerBase : ScriptableObject, IInstaller
    {
        [Inject]
        DiContainer _container = null;

        protected DiContainer Container
        {
            get { return _container; }
        }

        bool IInstaller.IsEnabled
        {
            get { return true; }
        }

        public abstract void InstallBindings();
    }
}

#endif

