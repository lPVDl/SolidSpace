namespace SpaceSimulator.Entities.Physics
{
    public class CollidersInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.Bind<ColliderBakeSystem>();
        }
    }
}