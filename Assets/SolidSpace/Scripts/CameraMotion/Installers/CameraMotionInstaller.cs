namespace SolidSpace.CameraMotion
{
    public class CameraMotionInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings(IContainer container)
        {
            container.Bind<CameraMotionController>();
        }
    }
}