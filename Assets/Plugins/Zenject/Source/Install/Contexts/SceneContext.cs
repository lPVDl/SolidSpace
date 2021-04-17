#if !NOT_UNITY3D

using System.Collections.Generic;
using UnityEngine;

namespace Zenject
{
    public class SceneContext : MonoBehaviour, IContext
    {
        public IEnumerable<GameObject> RootGameObjects => gameObject.scene.GetRootGameObjects();

        public GameObject GameObject => gameObject;

        public Transform Transform => gameObject.transform;

        DiContainer _container;

        [SerializeField] private List<ScriptableObjectInstallerBase> _installers;

        public void Awake()
        {
            using (ProfileBlock.Start("Zenject.SceneContext.Install"))
            {
                Install();
            }
            using (ProfileBlock.Start("Zenject.SceneContext.Resolve"))
            {
                Resolve();
            }
        }

        public void Validate()
        {
            Install();
            Resolve();
        }

        public void Install()
        {
            _container = new DiContainer();
            _container.DefaultParent = transform;
            _container.IsInstalling = true;
            
            try
            {
                InstallBindings();
            }
            finally
            {
                _container.IsInstalling = false;
            }
        }

        private void Resolve()
        {
            _container.ResolveRoots();
        }

        void InstallBindings()
        {
            _container.Bind<TickableManager>().AsSingle().NonLazy();
            _container.Bind<InitializableManager>().AsSingle().NonLazy();
            _container.Bind<DisposableManager>().AsSingle().NonLazy();
            _container.Bind<IContext>().FromInstance(this);
            _container.Bind(typeof(SceneKernel), typeof(MonoKernel)).To<SceneKernel>().FromNewComponentOn(gameObject).AsSingle().NonLazy();

            foreach (var installer in _installers)
            {
                _container.Inject(installer);
            }

            foreach (var installer in _installers)
            {
                installer.InstallBindings();
            }
        }
    }
}

#endif
