using SolidSpace.Entities.Health.Atlases.Interfaces;
using SolidSpace.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Health
{
    public interface IHealthAtlasSystem : ILinearAtlas<byte>
    {
        public void Copy(Texture2D source, AtlasIndex target);
    }
}