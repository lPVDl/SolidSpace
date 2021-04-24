using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SolidSpace.Entities.Rendering.Sprites
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SpriteVertexData
    {
        public float2 position;
        public half2 uv;
    }
}