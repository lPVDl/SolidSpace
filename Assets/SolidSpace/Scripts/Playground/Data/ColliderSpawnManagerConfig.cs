using UnityEngine;

namespace SolidSpace.Playground
{
       [System.Serializable]
       public class ColliderSpawnManagerConfig
       {
              public Vector2 ColliderWidth => _colliderWidth;
              public Vector2 ColliderHeight => _colliderHeight;
              public float OnStartSpawnCount => _onStartSpawnCount;
              public int SpawnPerSpawn => _spawnPerSpawn;
              public float SpawnExtraRadius => _spawnExtraRadius;
              public Vector2 SpawnRangeX => _spawnRangeX;
              public Vector2 SpawnRangeY => _spawnRangeY;

              [SerializeField] private Vector2 _colliderWidth;
              [SerializeField] private Vector2 _colliderHeight;
              [SerializeField] private float _onStartSpawnCount;
              [SerializeField] private int _spawnPerSpawn;
              [SerializeField] private float _spawnExtraRadius;
              [SerializeField] private Vector2 _spawnRangeX;
              [SerializeField] private Vector2 _spawnRangeY;
       }
}