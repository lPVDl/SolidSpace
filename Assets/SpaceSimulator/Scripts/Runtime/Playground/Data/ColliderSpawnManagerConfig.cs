using UnityEngine;

namespace SpaceSimulator.Runtime.Playground
{
    [System.Serializable]
    public class ColliderSpawnManagerConfig
    {
        [Serialize] public float ColliderRadius { get; private set; }
        [Serialize] public float OnStartSpawnCount { get; private set; }
        [Serialize] public int SpawnPerSpawn { get; private set; }
        [Serialize] public float SpawnExtraRadius { get; private set; }
        [Serialize] public Vector2 SpawnRangeX { get; private set; }
        [Serialize] public Vector2 SpawnRangeY { get; private set; }
        [Serialize] public bool DrawGizmos { get; private set; }
    }
}