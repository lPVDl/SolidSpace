using SpaceSimulator.Controllers;
using SpaceSimulator.Data;

namespace SpaceSimulator.Installers
{
    public class GameCycleInstaller : ScriptableObjectInstaller
    {
        [Serialize] private GameCycleConfig _gameCycleConfig;

        public override void InstallBindings(IContainer container)
        {
            container.Bind<InitializationController>(_gameCycleConfig);
            container.Bind<UpdatingController>(_gameCycleConfig);
            container.Bind<FinalizationController>();
        }
    }
}