namespace SpaceSimulator
{
    public abstract class ScriptableObjectInstaller : SolidScriptableObject
    {
        public abstract void InstallBindings(IContainer container);
    }
}