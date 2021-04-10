using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Profiling;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    [UpdateBefore(typeof(SpriteRenderSystem))]
    public class SpriteCommandSystem : SystemBase
    {
        private struct CopyTextureCommand
        {
            public Texture2D source;
            public SpriteIndex target;
        }

        private List<CopyTextureCommand> _commands;
        private SpriteAtlasSystem _atlasSystem;
        private SpriteIndexSystem _indexSystem;

        public void ScheduleTextureCopy(Texture2D source, SpriteIndex target)
        {
            _commands.Add(new CopyTextureCommand
            {
                source = source,
                target = target
            });
        }

        protected override void OnCreate()
        {
            _commands = new List<CopyTextureCommand>();
            _atlasSystem = World.GetOrCreateSystem<SpriteAtlasSystem>();
            _indexSystem = World.GetOrCreateSystem<SpriteIndexSystem>();
        }

        protected override void OnUpdate()
        {
            var atlasTextures = _atlasSystem.Textures;

            var chunks = _indexSystem.Chunks;
            
            Profiler.BeginSample("Copy Textures");
            for (var i = 0; i < _commands.Count; i++)
            {
                var command = _commands[i];
                var source = command.source;
                var sprite = command.target;
                var chunk = chunks[sprite.chunkId];
                var offsetX = chunk.offsetX << 2 + (sprite.itemId & 15) * (1 << chunk.power);
                var offsetY = chunk.offsetY << 2 + (sprite.itemId >> 4) * (1 << chunk.power);
                
                Graphics.CopyTexture(source, 0, 0, 0, 0, source.width, source.height, 
                    atlasTextures[chunk.atlasId], 0, 0, offsetX, offsetY);
            }
            Profiler.EndSample();

            _commands.Clear();
        }
    }
}