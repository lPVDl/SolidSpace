using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace SpaceSimulator.Entities.Rendering.Particles
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ParticleVertexData
    {
        public float2 position;
    }
}