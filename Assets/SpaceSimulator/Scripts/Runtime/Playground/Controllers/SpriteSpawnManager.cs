using System.IO;
using SpaceSimulator.Runtime.Entities.SpriteRendering;
using UnityEngine;

namespace SpaceSimulator.Runtime.Playground
{
    public class SpriteSpawnManager : IInitializable
    {
        private readonly SpriteSpawnManagerConfig _config;

        public EControllerType ControllerType => EControllerType.Common;
        
        public SpriteSpawnManager(SpriteSpawnManagerConfig config)
        {
            _config = config;
        }
        
        public void Initialize()
        {
            var colorSystem = new SpriteAtlasColorSystem();
            var commandSystem = new SpriteAtlasCommandSystem(colorSystem);

            var spriteTexture = _config.SpriteTexture;
            var spriteIndex = colorSystem.AllocateSpace(spriteTexture.width, spriteTexture.height);
            commandSystem.ScheduleTextureCopy(spriteTexture, spriteIndex);
            commandSystem.ProcessCommands();

            File.WriteAllBytes(_config.OutputAtlasPath, colorSystem.Texture.EncodeToPNG());
            
            colorSystem.Dispose();
        }
    }
}