using System;
using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.Entities.Splitting
{
    [BurstCompile]
    public struct ShapeReadJob : IJob
    {
        [ReadOnly] public NativeSlice<byte2> inConnections;
        [ReadOnly] public int inSeedCount;
        [ReadOnly] public int inConnectionCount;

        public NativeSlice<ByteBounds> inOutBounds;
        
        [WriteOnly] public NativeSlice<byte> outShapeRootSeeds;
        [WriteOnly] public NativeReference<int> outShapeCount;

        private Mask256 _connectionUsageMask;
        private Mask256 _seedUsageMask;
        
        public void Execute()
        {
            var shapeCount = 0;

            for (var seed = 1; seed <= inSeedCount; seed++)
            {
                if (_seedUsageMask.HasBit((byte) (seed - 1)))
                {
                    continue;
                }

                outShapeRootSeeds[shapeCount] = (byte) seed;
                var shapeBounds = inOutBounds[seed];

                for (var connectionIndex = 0; connectionIndex < inConnectionCount; connectionIndex++)
                {
                    if (_connectionUsageMask.HasBit((byte) connectionIndex))
                    {
                        continue;
                    }

                    var connection = inConnections[connectionIndex];
                    if (connection.x != seed)
                    {
                        continue;
                    }
                    
                    _connectionUsageMask.SetBitTrue((byte) connectionIndex);
                    _seedUsageMask.SetBitTrue((byte) (connection.y - 1));

                    shapeBounds = JoinBounds(shapeBounds, inOutBounds[connection.y]);
                }

                inOutBounds[shapeCount++] = shapeBounds;
            }

            outShapeCount.Value = shapeCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ByteBounds JoinBounds(ByteBounds a, ByteBounds b)
        {
            return new ByteBounds
            {
                min = new byte2
                {
                    x = Math.Min(a.min.x, b.min.x),
                    y = Math.Min(a.min.y, b.min.y),
                },
                max = new byte2
                {
                    x = Math.Max(a.max.x, b.max.x),
                    y = Math.Max(a.max.y, b.max.y)
                }
            };
        }
    }
}