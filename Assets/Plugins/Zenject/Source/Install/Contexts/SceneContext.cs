#if !NOT_UNITY3D

using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject.Internal;

namespace Zenject
{
    public class SceneContext : RunnableContext
    {
        public static Action<DiContainer> ExtraBindingsInstallMethod;
        public static Action<DiContainer> ExtraBindingsLateInstallMethod;

        public static IEnumerable<DiContainer> ParentContainers;

        [FormerlySerializedAs("ParentNewObjectsUnderRoot")]
        [FormerlySerializedAs("_parentNewObjectsUnderRoot")]
        [Tooltip("When true, objects that are created at runtime will be parented to the SceneContext")]
        [SerializeField]
        bool _parentNewObjectsUnderSceneContext;

        [Tooltip("Optional contract names for this SceneContext, allowing contexts in subsequently loaded scenes to depend on it and be parented to it, and also for previously loaded decorators to be included")]
        [SerializeField]
        List<string> _contractNames = new List<string>();

        [Tooltip("Optional contract names of SceneContexts in previously loaded scenes that this context depends on and to which it should be parented")]
        [SerializeField]
        List<string> _parentContractNames = new List<string>();

        DiContainer _container;

        bool _hasInstalled;
        bool _hasResolved;

        public override DiContainer Container
        {
            get { return _container; }
        }

        public bool IsValidating
        {
            get
            {
                return ProjectContext.Instance.Container.IsValidating;
            }
        }

        public IEnumerable<string> ContractNames
        {
            get { return _contractNames; }
            set
            {
                _contractNames.Clear();
                _contractNames.AddRange(value);
            }
        }

        public IEnumerable<string> ParentContractNames
        {
            get
            {
                var result = new List<string>();
                result.AddRange(_parentContractNames);
                return result;
            }
            set
            {
                _parentContractNames = value.ToList();
            }
        }

        public bool ParentNewObjectsUnderSceneContext
        {
            get { return _parentNewObjectsUnderSceneContext; }
            set { _parentNewObjectsUnderSceneContext = value; }
        }

        public void Awake()
        {
#if ZEN_INTERNAL_PROFILING
            ProfileTimers.ResetAll();
            using (ProfileTimers.CreateTimedBlock("Other"))
#endif
            {
                Initialize();
            }
        }

        public void Validate()
        {
            Assert.That(IsValidating);

            Install();
            Resolve();
        }

        protected override void RunInternal()
        {
            // We always want to initialize ProjectContext as early as possible
            ProjectContext.Instance.EnsureIsInitialized();

#if UNITY_EDITOR
            using (ProfileBlock.Start("Zenject.SceneContext.Install"))
#endif
            {
                Install();
            }

#if UNITY_EDITOR
            using (ProfileBlock.Start("Zenject.SceneContext.Resolve"))
#endif
            {
                Resolve();
            }
        }

        public override IEnumerable<GameObject> GetRootGameObjects()
        {
            return ZenUtilInternal.GetRootGameObjects(gameObject.scene);
        }

        IEnumerable<DiContainer> GetParentContainers()
        {
            var parentContractNames = ParentContractNames;

            if (parentContractNames.IsEmpty())
            {
                if (ParentContainers != null)
                {
                    var tempParentContainer = ParentContainers;

                    // Always reset after using it - it is only used to pass the reference
                    // between scenes via ZenjectSceneLoader
                    ParentContainers = null;

                    return tempParentContainer;
                }

                return new[] { ProjectContext.Instance.Container };
            }

            Assert.IsNull(ParentContainers,
                "Scene cannot have both a parent scene context name set and also an explicit parent container given");

            var parentContainers = UnityUtil.AllLoadedScenes
                .Except(gameObject.scene)
                .SelectMany(scene => scene.GetRootGameObjects())
                .SelectMany(root => root.GetComponentsInChildren<SceneContext>())
                .Where(sceneContext => sceneContext.ContractNames.Where(x => parentContractNames.Contains(x)).Any())
                .Select(x => x.Container)
                .ToList();

            if (!parentContainers.Any())
            {
                throw Assert.CreateException(
                    "SceneContext on object {0} of scene {1} requires at least one of contracts '{2}', but none of the loaded SceneContexts implements that contract.",
                    gameObject.name,
                    gameObject.scene.name,
                    parentContractNames.Join(", "));
            }

            return parentContainers;
        }

        public void Install()
        {
            Assert.That(!_hasInstalled);
            _hasInstalled = true;

            Assert.IsNull(_container);

            var parents = GetParentContainers();

            _container = new DiContainer(parents, parents.First().IsValidating);

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

        public void Resolve()
        {
            _hasResolved = true;

            _container.ResolveRoots();
        }

        void InstallBindings()
        {
            _container.Bind(typeof(Context), typeof(SceneContext)).To<SceneContext>().FromInstance(this);

            _container.Bind(typeof(SceneKernel), typeof(MonoKernel))
                .To<SceneKernel>().FromNewComponentOn(gameObject).AsSingle().NonLazy();

            InstallInstallers();
        }

        protected override void GetInjectableMonoBehaviours(List<MonoBehaviour> monoBehaviours)
        {
            monoBehaviours.Clear();
        }
    }
}

#endif
