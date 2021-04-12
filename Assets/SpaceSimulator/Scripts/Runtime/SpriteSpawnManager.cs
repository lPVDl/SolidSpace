using System.Collections;
using System.IO;
using SpaceSimulator.Runtime.Entities.SpriteRendering;
using UnityEngine;

namespace SpaceSimulator.Runtime
{
    public class SpriteSpawnManager : MonoBehaviour
    {
        [SerializeField] private Texture2D _spriteTexture;
        [SerializeField] private string _outputAtlasPath;

        private IEnumerator Start()
        {
            var colorSystem = new SpriteAtlasColorSystem();
            var commandSystem = new SpriteAtlasCommandSystem(colorSystem);
            
            var spriteIndex = colorSystem.AllocateSpace(_spriteTexture.width, _spriteTexture.height);
            commandSystem.ScheduleTextureCopy(_spriteTexture, spriteIndex);
            commandSystem.ProcessCommands();

            yield return new WaitForSeconds(1);
            
            File.WriteAllBytes(_outputAtlasPath, colorSystem.Texture.EncodeToPNG());
            
            colorSystem.Dispose();
        }
    }
}