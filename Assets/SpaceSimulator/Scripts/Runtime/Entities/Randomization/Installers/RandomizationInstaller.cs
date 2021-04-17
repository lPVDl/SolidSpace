namespace SpaceSimulator.Runtime.Entities.Randomization
{
    public class RandomizationInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<RandomValueSystem>().AsSingle();
        }
    }
}