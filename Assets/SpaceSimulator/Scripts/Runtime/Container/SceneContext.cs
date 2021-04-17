using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SpaceSimulator.Runtime
{
    public class SceneContext : MonoBehaviour, IContext
    {
        public IEnumerable<GameObject> RootGameObjects => gameObject.scene.GetRootGameObjects();

        public GameObject GameObject => gameObject;

        public Transform Transform => gameObject.transform;

        [SerializeField] private List<ScriptableObjectInstaller> _installers;

        public void Awake()
        {
            var container = new DiContainer();
            container.DefaultParent = transform;
            container.IsInstalling = true;
            
            InstallBindings(container);

            container.IsInstalling = false;
            
            container.ResolveRoots();
        }

        void InstallBindings(DiContainer container)
        {
            container.Bind<IContext>().FromInstance(this);
            container.Bind<TickableManager>().AsSingle().NonLazy();
            container.Bind<InitializableManager>().AsSingle().NonLazy();
            container.Bind<DisposableManager>().AsSingle().NonLazy();
            container.Bind(typeof(SceneKernel), typeof(MonoKernel)).To<SceneKernel>().FromNewComponentOn(gameObject).AsSingle().NonLazy();

            foreach (var installer in _installers)
            {
                installer.InstallBindings(container);
            }
        }
    }
}
