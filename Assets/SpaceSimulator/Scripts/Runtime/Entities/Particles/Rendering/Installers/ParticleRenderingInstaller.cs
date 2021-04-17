using Zenject;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    public class ParticleRenderingInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<ParticleMeshSystem>().AsSingle();
        }
    }
}