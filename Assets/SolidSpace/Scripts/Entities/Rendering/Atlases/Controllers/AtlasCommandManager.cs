using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Atlases
{
    public class AtlasCommandManager
    {
        private struct CopyTextureCommand
        {
            public Texture2D source;
            public AtlasIndex target;
        }
        
        private readonly IAtlasSystem _atlas;
        private readonly List<CopyTextureCommand> _commands;

        private AtlasMathUtil _atlasMath;
        
        public AtlasCommandManager(IAtlasSystem atlas)
        {
            _atlas = atlas;
            _commands = new List<CopyTextureCommand>();
        }

        public void ScheduleTextureCopy(Texture2D source, AtlasIndex target)
        {
            _commands.Add(new CopyTextureCommand
            {
                source = source,
                target = target
            });
        }

        public void ProcessCommands()
        {
            for (var i = 0; i < _commands.Count; i++)
            {
                var command = _commands[i];
                var sprite = command.target;
                var offset = _atlasMath.ComputeOffset(_atlas.Chunks[sprite.chunkId], sprite);
                var source = command.source;
                
                Graphics.CopyTexture(source, 0, 0, 0, 0, source.width, source.height, 
                    _atlas.Texture, 0, 0, offset.x, offset.y);
            }
            
            _commands.Clear();
        }
    }
}