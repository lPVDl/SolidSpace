using UnityEngine;
using Zenject;

namespace SpaceSimulator.Runtime
{
    public class GameCycleInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private GameCycleConfig _gameCycleConfig;

        public override void InstallBindings(DiContainer container)
        {
            container.BindInterfacesTo<InitializationController>().AsSingle().WithArguments(_gameCycleConfig);
            container.BindInterfacesTo<UpdatingController>().AsSingle().WithArguments(_gameCycleConfig);
            container.BindInterfacesTo<FinalizationController>().AsSingle();
        }
    }
}