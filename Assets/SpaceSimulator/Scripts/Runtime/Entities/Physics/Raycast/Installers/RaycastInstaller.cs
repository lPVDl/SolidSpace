namespace SpaceSimulator.Runtime.Entities.Physics
{
    public class RaycastInstaller : ScriptableInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<RaycastComputeSystem>().AsSingle();
            Container.BindInterfacesTo<RaycastCommandSystem>().AsSingle();
        }
    }
}