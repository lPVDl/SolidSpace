using Zenject;

namespace SpaceSimulator.Runtime.Entities.Despawn
{
    public class DespawnInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<DespawnComputeSystem>().AsSingle();
            container.BindInterfacesTo<DespawnCommandSystem>().AsSingle();
        }
    }
}