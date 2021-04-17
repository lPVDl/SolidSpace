namespace SpaceSimulator.Runtime.Entities.Physics
{
    public class CollidersInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.BindInterfacesAndSelfTo<ColliderBakeSystem>();
        }
    }
}