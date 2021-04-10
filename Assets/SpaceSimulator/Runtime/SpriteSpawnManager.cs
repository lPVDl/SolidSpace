using System.Collections;
using System.IO;
using SpaceSimulator.Runtime.Entities.SpriteRendering;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SpaceSimulator.Runtime
{
    public class SpriteSpawnManager : MonoBehaviour
    {
        [SerializeField] private Texture2D _spriteTexture;
        [SerializeField] private string _outputAtlasPath;

        private IEnumerator Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var indexSystem = world.GetOrCreateSystem<SpriteIndexSystem>();
            var commandSystem = world.GetOrCreateSystem<SpriteCommandSystem>();
            
            var spriteIndex = indexSystem.AllocateSpace(new int2(_spriteTexture.width, _spriteTexture.height));
            commandSystem.ScheduleTextureCopy(_spriteTexture, spriteIndex);

            yield return new WaitForSeconds(1);

            var atlasSystem = world.GetOrCreateSystem<SpriteAtlasSystem>();
            File.WriteAllBytes(_outputAtlasPath, atlasSystem.Textures[0].EncodeToPNG());
        }
    }
}