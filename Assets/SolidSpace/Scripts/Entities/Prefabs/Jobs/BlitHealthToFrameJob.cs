using SolidSpace.Entities.Health;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Prefabs
{
    [BurstCompile]
    public struct BlitHealthToFrameJob : IJob
    {
        [ReadOnly] public int3 inAtlasOffset;
        [ReadOnly] public int2 inAtlasSize;
        [ReadOnly] public int2 inHealthSize;
        [ReadOnly] public NativeSlice<byte> inHealth;

        public NativeArray<float> inOutAtlasTexture;

        public void Execute()
        {
            for (var y = 0; y < inHealthSize.y; y++)
            {
                for (var x = 0; x < inHealthSize.x; x++)
                {
                    var framePoint = (inAtlasOffset.y + y) * inAtlasSize.x + inAtlasOffset.x + x;
                    var frameValue = (int) inOutAtlasTexture[framePoint];
                    
                    if (HealthUtil.HasBit(inHealth, inHealthSize, new int2(x, y)))
                    {
                        frameValue |= 1 << inAtlasOffset.z;
                    }
                    else
                    {
                        frameValue &= ~(1 << inAtlasOffset.z);
                    }

                    inOutAtlasTexture[framePoint] = (ushort) frameValue;
                }
            }
        }
    }
}