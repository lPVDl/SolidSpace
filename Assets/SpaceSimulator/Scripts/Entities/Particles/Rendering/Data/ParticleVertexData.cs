using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SpaceSimulator.Entities.Particles.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ParticleVertexData
    {
        public float2 position;
    }
}