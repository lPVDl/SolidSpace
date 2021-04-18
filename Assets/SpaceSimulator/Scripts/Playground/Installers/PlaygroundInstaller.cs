using UnityEngine;

namespace SpaceSimulator.Playground
{
    public class PlaygroundInstaller : ScriptableObjectInstaller
    {
        [Serialize] private ColliderSpawnManagerConfig _colliderSpawnManagerConfig;
        [Serialize] private EmitterSpawnManagerConfig _emitterSpawnManagerConfig;
        [Serialize] private SpriteSpawnManagerConfig _spriteSpawnManagerConfig; 
        
        public override void InstallBindings(IContainer container)
        {
            container.BindFromComponentInHierarchy<Camera>();
            container.Bind<ColliderSpawnManager>(_colliderSpawnManagerConfig);
            container.Bind<EmitterSpawnManager>(_emitterSpawnManagerConfig);
            container.Bind<SpriteSpawnManager>(_spriteSpawnManagerConfig);
        }
    }
}