using Unity.Entities;
using UnityEngine;

namespace SpaceMassiveSimulator.Runtime.Entities.Particles.Rendering
{
    internal struct ParticleMeshEntityData
    {
        public Mesh mesh;
        public Entity entity;
        public bool visible;
    }
}