using SolidSpace.DependencyInjection;
using UnityEngine;

namespace SolidSpace.GameCycle
{
    internal class GameCycleInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private Config _config;

        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<GameCycleController>(_config);
        }
    }
}