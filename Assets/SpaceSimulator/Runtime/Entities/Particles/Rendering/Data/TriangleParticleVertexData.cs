using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TriangleParticleVertexData
    {
        public float2 position;
    }
}