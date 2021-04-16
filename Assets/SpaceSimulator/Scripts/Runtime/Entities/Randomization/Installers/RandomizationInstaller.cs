namespace SpaceSimulator.Runtime.Entities.Randomization
{
    public class RandomizationInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<RandomValueSystem>().AsSingle();
        }
    }
}