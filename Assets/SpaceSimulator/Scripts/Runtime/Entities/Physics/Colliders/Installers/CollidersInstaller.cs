namespace SpaceSimulator.Runtime.Entities.Physics
{
    public class CollidersInstaller : ScriptableInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ColliderBakeSystem>().AsSingle();
        }
    }
}