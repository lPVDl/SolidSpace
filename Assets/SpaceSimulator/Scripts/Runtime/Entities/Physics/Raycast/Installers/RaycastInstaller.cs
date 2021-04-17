namespace SpaceSimulator.Runtime.Entities.Physics
{
    public class RaycastInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.BindInterfacesAndSelfTo<RaycastComputeSystem>();
            container.BindInterfacesTo<RaycastCommandSystem>();
        }
    }
}