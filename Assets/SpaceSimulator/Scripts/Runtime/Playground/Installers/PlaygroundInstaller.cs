using UnityEngine;

namespace SpaceSimulator.Runtime.Playground
{
    public class PlaygroundInstaller : ScriptableInstaller
    {
        [SerializeField] private ColliderSpawnManagerConfig _colliderSpawnManagerConfig;
        [SerializeField] private EmitterSpawnManagerConfig _emitterSpawnManagerConfig;
        [SerializeField] private SpriteSpawnManagerConfig _spriteSpawnManagerConfig; 
        
        public override void InstallBindings()
        {
            Container.Bind<Camera>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesTo<ColliderSpawnManager>().AsSingle().WithArguments(_colliderSpawnManagerConfig);
            Container.BindInterfacesTo<EmitterSpawnManager>().AsSingle().WithArguments(_emitterSpawnManagerConfig);
            Container.BindInterfacesTo<SpriteSpawnManager>().AsSingle().WithArguments(_spriteSpawnManagerConfig);
        }
    }
}