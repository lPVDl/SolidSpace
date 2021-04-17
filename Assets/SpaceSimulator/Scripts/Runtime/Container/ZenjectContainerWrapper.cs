using Zenject;

namespace SpaceSimulator.Runtime
{
    public class ZenjectContainerWrapper : IContainer
    {
        private readonly DiContainer _container;

        public ZenjectContainerWrapper(DiContainer container)
        {
            _container = container;
        }

        public void BindInterfacesAndSelfTo<T>()
        {
            _container.BindInterfacesAndSelfTo<T>().AsSingle();
        }

        public void BindInterfacesTo<T>()
        {
            _container.BindInterfacesTo<T>().AsSingle();
        }

        public void BindInterfacesTo<T>(object parameter)
        {
            _container.BindInterfacesTo<T>().AsSingle().WithArguments(parameter);
        }

        public void BindFromComponentInHierarchy<T>()
        {
            _container.Bind<T>().FromComponentInHierarchy().AsSingle();
        }
    }
}