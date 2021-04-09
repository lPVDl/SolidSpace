using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public class SpriteAtlasSystem : SystemBase
    {
        public IReadOnlyList<Texture2D> Textures => _textures;
        
        private List<Texture2D> _textures;

        public byte CreateTexture(int2 size)
        {
            var texture = new Texture2D(size.x, size.y, TextureFormat.ARGB32, false);
            _textures.Add(texture);
            
            return (byte) (_textures.Count - 1);
        } 
        
        protected override void OnCreate()
        {
            _textures = new List<Texture2D>();
        }

        protected override void OnUpdate()
        {
            
        }

        protected override void OnDestroy()
        {
            for (var i = 0; i < _textures.Count; i++)
            {
                Object.Destroy(_textures[i]);
                _textures[i] = null;
            }

            _textures = null;
        }
    }
}