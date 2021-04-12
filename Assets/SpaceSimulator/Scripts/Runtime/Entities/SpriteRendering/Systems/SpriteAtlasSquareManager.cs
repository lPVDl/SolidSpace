using System;
using System.Collections.Generic;

namespace SpaceSimulator.Runtime.Entities.SpriteRendering
{
    public class SpriteAtlasSquareManager
    {
        private readonly Stack<SpriteAtlasSquare>[] _emptySquares;
        private readonly byte _atlasPower;
        
        public SpriteAtlasSquareManager(byte atlasPower)
        {
            _atlasPower = atlasPower;
            _emptySquares = new Stack<SpriteAtlasSquare>[atlasPower + 1];
            
            for (var i = 0; i <= atlasPower; i++)
            {
                _emptySquares[i] = new Stack<SpriteAtlasSquare>();
            }
            
            _emptySquares[atlasPower].Push(new SpriteAtlasSquare
            {
                offsetX = 0,
                offsetY = 0,
            });
        }

        public SpriteAtlasSquare AllocateSquare(int power)
        {
            var squareStack = _emptySquares[power];
            if (squareStack.Count > 0)
            {
                return squareStack.Pop();
            }

            for (var i = power + 1; i <= _atlasPower; i++)
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
                        offsetX = (byte)(square.offsetX + size),
                        offsetY = square.offsetY
                    });
                    squareStack.Push(new SpriteAtlasSquare
                    {
                        offsetX = square.offsetX,
                        offsetY = (byte)(square.offsetY + size)
                    });
                    squareStack.Push(new SpriteAtlasSquare
                    {
                        offsetX = (byte)(square.offsetX + size),
                        offsetY = (byte)(square.offsetY + size)
                    });
                }

                return square;
            }

            throw new OutOfMemoryException($"Failed to allocate square with size {1 << power}");
        }
    }
}