using SpaceSimulator.CameraMotion.Controllers;

namespace SpaceSimulator.Cinematics.Installers
{
    public class CameraMotionInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.Bind<CameraMotionController>();
        }
    }
}