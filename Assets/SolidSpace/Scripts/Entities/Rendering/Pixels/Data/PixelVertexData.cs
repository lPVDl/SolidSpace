using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SolidSpace.Entities.Rendering.Pixels
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PixelVertexData
    {
        public float2 position;
    }
}