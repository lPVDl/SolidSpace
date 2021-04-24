namespace SolidSpace.Entities.Physics
{
    public class RaycastInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.Bind<RaycastComputeSystem>();
            container.Bind<RaycastCommandSystem>();
        }
    }
}