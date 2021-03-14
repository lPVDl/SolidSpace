using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SpaceMassiveSimulator.Runtime.Entities.Particles
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TriangleParticleVertexData
    {
        public float2 position;
    }
}