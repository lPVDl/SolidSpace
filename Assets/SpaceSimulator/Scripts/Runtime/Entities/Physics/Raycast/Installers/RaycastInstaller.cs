using Zenject;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public class RaycastInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<RaycastComputeSystem>().AsSingle();
            container.BindInterfacesTo<RaycastCommandSystem>().AsSingle();
        }
    }
}