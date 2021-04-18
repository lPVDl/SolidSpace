namespace SpaceSimulator
{
    public interface IContainer
    {
        void Bind<T>();
        void Bind<T>(object parameter);
        void BindFromComponentInHierarchy<T>();
    }
}