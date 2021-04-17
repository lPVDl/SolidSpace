using Zenject;

namespace SpaceSimulator.Runtime.Entities.Physics.Velocity
{
    public class VelocityInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(DiContainer container)
        {
            container.BindInterfacesTo<VelocitySystem>().AsSingle();
        }
    }
}