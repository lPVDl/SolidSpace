using UnityEngine;

namespace SpaceSimulator.Runtime.Playground
{
    [System.Serializable]
    public class ColliderSpawnManagerConfig
    {
        public float ColliderRadius => _colliderRadius;
        public float OnStartSpawnCount => _onStartSpawnCount;
        public int SpawnPerSpawn => _spawnPerSpawn;
        public float SpawnExtraRadius => _spawnExtraRadius;
        public Vector2 SpawnRangeX => _spawnRangeX;
        public Vector2 SpawnRangeY => _spawnRangeY;
        public bool DrawGizmos => _drawGizmos;
        
        [SerializeField] private float _colliderRadius;
        [SerializeField] private int _onStartSpawnCount;
        [SerializeField] private int _spawnPerSpawn;
        [SerializeField] private float _spawnExtraRadius;
        [SerializeField] private Vector2 _spawnRangeX;
        [SerializeField] private Vector2 _spawnRangeY;
        [SerializeField] private bool _drawGizmos;
    }
}