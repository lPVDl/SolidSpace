namespace SpaceSimulator.Runtime.Entities.Physics.Velocity
{
    public class VelocityInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.BindInterfacesTo<VelocitySystem>();
        }
    }
}