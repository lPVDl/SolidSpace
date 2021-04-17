using UnityEngine;

namespace SpaceSimulator.Runtime.Playground
{
    public class PlaygroundInstaller : ScriptableObjectInstaller
    {
        [SerializeField] private ColliderSpawnManagerConfig _colliderSpawnManagerConfig;
        [SerializeField] private EmitterSpawnManagerConfig _emitterSpawnManagerConfig;
        [SerializeField] private SpriteSpawnManagerConfig _spriteSpawnManagerConfig; 
        
        public override void InstallBindings(IContainer container)
        {
            container.BindFromComponentInHierarchy<Camera>();
            container.BindInterfacesTo<ColliderSpawnManager>(_colliderSpawnManagerConfig);
            container.BindInterfacesTo<EmitterSpawnManager>(_emitterSpawnManagerConfig);
            container.BindInterfacesTo<SpriteSpawnManager>(_spriteSpawnManagerConfig);
        }
    }
}