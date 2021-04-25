using UnityEngine;

namespace SolidSpace
{
    public class GameCycleInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private GameCycleConfig _gameCycleConfig;

        public override void InstallBindings(IContainer container)
        {
            container.Bind<GameCycleController>(_gameCycleConfig);
        }
    }
}