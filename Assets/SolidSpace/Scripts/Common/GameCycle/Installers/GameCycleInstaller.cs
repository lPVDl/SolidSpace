using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace
{
    public class GameCycleInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private GameCycleConfig _gameCycleConfig;

        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<GameCycleController>(_gameCycleConfig);
        }
    }
}