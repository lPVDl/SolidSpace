namespace SpaceSimulator.Runtime.Entities.Randomization
{
    public class RandomizationInstaller : ScriptableInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<RandomValueSystem>().AsSingle();
        }
    }
}