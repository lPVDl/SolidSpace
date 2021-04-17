using UnityEngine;

namespace SpaceSimulator.Runtime
{
    public class GameCycleInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private GameCycleConfig _gameCycleConfig;

        public override void InstallBindings(IContainer container)
        {
            container.BindInterfacesTo<InitializationController>(_gameCycleConfig);
            container.BindInterfacesTo<UpdatingController>(_gameCycleConfig);
            container.BindInterfacesTo<FinalizationController>();
        }
    }
}