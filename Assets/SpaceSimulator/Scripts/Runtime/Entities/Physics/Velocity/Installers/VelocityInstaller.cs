namespace SpaceSimulator.Runtime.Entities.Physics
{
    public class VelocityInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.Bind<VelocitySystem>();
        }
    }
}