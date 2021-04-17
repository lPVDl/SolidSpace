namespace SpaceSimulator.Runtime.Entities.Physics
{
    public class CollidersInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ColliderBakeSystem>().AsSingle();
        }
    }
}