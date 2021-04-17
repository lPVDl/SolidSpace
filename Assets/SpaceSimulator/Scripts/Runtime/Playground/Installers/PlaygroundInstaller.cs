using UnityEngine;
using Zenject;

namespace SpaceSimulator.Runtime.Playground
{
    public class PlaygroundInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private ColliderSpawnManagerConfig _colliderSpawnManagerConfig;
        [SerializeField] private EmitterSpawnManagerConfig _emitterSpawnManagerConfig;
        [SerializeField] private SpriteSpawnManagerConfig _spriteSpawnManagerConfig; 
        
        public override void InstallBindings(DiContainer container)
        {
            container.Bind<Camera>().FromComponentInHierarchy().AsSingle();
            container.BindInterfacesTo<ColliderSpawnManager>().AsSingle().WithArguments(_colliderSpawnManagerConfig);
            container.BindInterfacesTo<EmitterSpawnManager>().AsSingle().WithArguments(_emitterSpawnManagerConfig);
            container.BindInterfacesTo<SpriteSpawnManager>().AsSingle().WithArguments(_spriteSpawnManagerConfig);
        }
    }
}