namespace SpaceSimulator.Runtime.Entities.Physics.Velocity
{
    public class VelocityInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<VelocitySystem>().AsSingle();
        }
    }
}