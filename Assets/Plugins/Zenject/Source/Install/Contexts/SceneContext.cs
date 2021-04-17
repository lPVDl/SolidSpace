#if !NOT_UNITY3D

using System.Collections.Generic;
using UnityEngine;
using Zenject.Internal;

namespace Zenject
{
    public class SceneContext : Context
    {
        DiContainer _container;

        public override DiContainer Container
        {
            get { return _container; }
        }

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

        public override IEnumerable<GameObject> GetRootGameObjects()
        {
            return gameObject.scene.GetRootGameObjects();
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
            _container.Bind(typeof(Context), typeof(SceneContext)).To<SceneContext>().FromInstance(this);
            _container.Bind(typeof(SceneKernel), typeof(MonoKernel)).To<SceneKernel>().FromNewComponentOn(gameObject).AsSingle().NonLazy();

            InstallInstallers();
        }

        protected override void GetInjectableMonoBehaviours(List<MonoBehaviour> monoBehaviours)
        {
            monoBehaviours.Clear();
        }
    }
}

#endif
