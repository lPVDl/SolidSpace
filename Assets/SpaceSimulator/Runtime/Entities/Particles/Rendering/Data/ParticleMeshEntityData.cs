using Unity.Entities;
using UnityEngine;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    internal struct ParticleMeshEntityData
    {
        public Mesh mesh;
        public Entity entity;
        public bool visible;
    }
}