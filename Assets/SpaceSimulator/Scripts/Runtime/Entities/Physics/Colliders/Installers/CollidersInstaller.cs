using Zenject;

namespace SpaceSimulator.Runtime.Entities.Physics
{
    public class CollidersInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<ColliderBakeSystem>().AsSingle();
        }
    }
}