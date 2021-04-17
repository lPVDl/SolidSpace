namespace SpaceSimulator.Runtime.Entities.Randomization
{
    public class RandomizationInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.Bind<RandomValueSystem>();
        }
    }
}