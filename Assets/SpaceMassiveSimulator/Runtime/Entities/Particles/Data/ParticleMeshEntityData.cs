using Unity.Entities;
using UnityEngine;

namespace SpaceMassiveSimulator.Runtime.Entities.Particles
{
    internal struct ParticleMeshEntityData
    {
        public Mesh mesh;
        public Entity entity;
        public bool visible;
    }
}