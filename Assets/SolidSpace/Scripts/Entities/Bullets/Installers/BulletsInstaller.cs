using SolidSpace.DependencyInjection;

namespace SolidSpace.Entities.Bullets
{
    public class BulletsInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IDependencyContainer container)
        {
            container.Bind<BulletCommandSystem>();
        }
    }
}