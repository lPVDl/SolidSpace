using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public class SpriteSquareSystem : SystemBase
    {
        private const int MaxSquarePower = 10;
        private const int AtlasSize = 1 << MaxSquarePower;
        
        private Stack<SpriteAtlasSquare>[] _emptySquares;
        private SpriteAtlasSystem _atlasSystem;
        
        protected override void OnCreate()
        {
            _atlasSystem = World.GetOrCreateSystem<SpriteAtlasSystem>();
            _emptySquares = new Stack<SpriteAtlasSquare>[MaxSquarePower + 1];
            for (var i = 0; i <= MaxSquarePower; i++)
            {
                _emptySquares[i] = new Stack<SpriteAtlasSquare>();
            }
        }
        
        public SpriteAtlasSquare AllocateSquare(byte power)
        {
            var squareStack = _emptySquares[power];
            if (squareStack.Count > 0)
            {
                return squareStack.Pop();
            }

            for (var i = power + 1; i <= MaxSquarePower; i++)
            {
                squareStack = _emptySquares[i];
                if (squareStack.Count == 0)
                {
                    continue;
                }

                var square = squareStack.Pop();
                for (var j = i - 1; j >= power; j--)
                {
                    var size = 1 << (j - 2);
                    squareStack = _emptySquares[j];
                    squareStack.Push(new SpriteAtlasSquare
                    {
                        atlasId = square.atlasId,
                        offsetX = (byte)(square.offsetX + size),
                        offsetY = square.offsetY
                    });
                    squareStack.Push(new SpriteAtlasSquare
                    {
                        atlasId = square.atlasId,
                        offsetX = square.offsetX,
                        offsetY = (byte)(square.offsetY + size)
                    });
                    squareStack.Push(new SpriteAtlasSquare
                    {
                        atlasId = square.atlasId,
                        offsetX = (byte)(square.offsetX + size),
                        offsetY = (byte)(square.offsetY + size)
                    });
                }

                return square;
            }
            
            _emptySquares[MaxSquarePower].Push(new SpriteAtlasSquare()
            {
                atlasId = _atlasSystem.CreateTexture(new int2(AtlasSize, AtlasSize)),
                offsetX = 0,
                offsetY = 0
            });
            
            return AllocateSquare(power);
        }

        protected override void OnUpdate()
        {
            
        }
    }
}