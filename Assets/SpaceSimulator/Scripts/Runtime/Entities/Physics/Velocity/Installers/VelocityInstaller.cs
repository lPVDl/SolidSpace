namespace SpaceSimulator.Runtime.Entities.Physics.Velocity
{
    public class VelocityInstaller : ScriptableInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<VelocitySystem>().AsSingle();
        }
    }
}