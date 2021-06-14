using SolidSpace.Entities.Health.Atlases;
using SolidSpace.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Health
{
    public interface IHealthAtlasSystem : ILinearAtlas<byte>
    {
        public void Copy(Texture2D source, AtlasIndex target);
    }
}