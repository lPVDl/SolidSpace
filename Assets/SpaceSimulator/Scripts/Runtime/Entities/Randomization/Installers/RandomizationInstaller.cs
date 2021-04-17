using Zenject;

namespace SpaceSimulator.Runtime.Entities.Randomization
{
    public class RandomizationInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(DiContainer container)
        {
            container.BindInterfacesTo<RandomValueSystem>().AsSingle();
        }
    }
}