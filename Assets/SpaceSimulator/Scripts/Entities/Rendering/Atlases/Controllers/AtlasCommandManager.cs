using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace SpaceSimulator.Entities.Rendering.Atlases
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
            Profiler.BeginSample("Copy Textures");
            for (var i = 0; i < _commands.Count; i++)
            {
                var command = _commands[i];
                var sprite = command.target;
                var chunk = _atlas.Chunks[sprite.chunkId];
                
                chunk.GetPower(out var indexPower, out var itemPower);
                var offsetX = chunk.offsetX << 2 + (sprite.itemId & ((1 << indexPower) - 1)) * (1 << itemPower);
                var offsetY = chunk.offsetY << 2 + (sprite.itemId >> indexPower) * (1 << itemPower);
                
                var source = command.source;
                Graphics.CopyTexture(source, 0, 0, 0, 0, source.width, source.height, 
                    _atlas.Texture, 0, 0, offsetX, offsetY);
            }
            Profiler.EndSample();
            
            _commands.Clear();
        }
    }
}