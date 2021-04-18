using UnityEngine;

namespace SpaceSimulator.Playground
{
    public class PlaygroundInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private ColliderSpawnManagerConfig _colliderSpawnManagerConfig;
        [SerializeField] private EmitterSpawnManagerConfig _emitterSpawnManagerConfig;
        [SerializeField] private SpriteSpawnManagerConfig _spriteSpawnManagerConfig; 
        
        public override void InstallBindings(IContainer container)
        {
            container.BindFromComponentInHierarchy<Camera>();
            container.Bind<ColliderSpawnManager>(_colliderSpawnManagerConfig);
            container.Bind<EmitterSpawnManager>(_emitterSpawnManagerConfig);
            container.Bind<SpriteSpawnManager>(_spriteSpawnManagerConfig);
        }
    }
}