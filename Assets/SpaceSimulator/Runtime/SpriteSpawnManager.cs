using SpaceSimulator.Runtime.Entities.SpriteRendering;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SpaceSimulator.Runtime
{
    public class SpriteSpawnManager : MonoBehaviour
    {
        private void Start()
        {
            var indexSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SpriteIndexSystem>();
            var spriteIndex0 = indexSystem.AllocateSpace(new int2(20, 15));
            var spriteIndex1 = indexSystem.AllocateSpace(new int2(12, 18));
            var spriteIndex2 = indexSystem.AllocateSpace(new int2(12, 14));
        }
    }
}