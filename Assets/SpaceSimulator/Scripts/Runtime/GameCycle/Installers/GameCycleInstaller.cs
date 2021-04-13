using UnityEngine;

namespace SpaceSimulator.Runtime
{
    public class GameCycleInstaller : ScriptableInstaller
    {
        [SerializeField] private GameCycleConfig _gameCycleConfig;

        public override void InstallBindings()
        {
            Container.BindInterfacesTo<InitializationController>().AsSingle().WithArguments(_gameCycleConfig);
            Container.BindInterfacesTo<UpdatingController>().AsSingle().WithArguments(_gameCycleConfig);
            Container.BindInterfacesTo<FinalizationController>().AsSingle();
        }
    }
}