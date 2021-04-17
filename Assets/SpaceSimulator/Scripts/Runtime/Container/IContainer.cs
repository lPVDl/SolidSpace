namespace SpaceSimulator.Runtime
{
    public interface IContainer
    {
        void BindInterfacesAndSelfTo<T>();
        void BindInterfacesTo<T>();
        void BindInterfacesTo<T>(object parameter);
        void BindFromComponentInHierarchy<T>();
    }
}