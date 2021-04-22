using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SpaceSimulator.Entities.Rendering.Pixels
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PixelVertexData
    {
        public float2 position;
    }
}