using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Randomization
{
    public class RandomizationInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<RandomValueSystem>();
        }
    }
}