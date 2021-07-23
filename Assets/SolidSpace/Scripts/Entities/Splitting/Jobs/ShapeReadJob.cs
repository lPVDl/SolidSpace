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
        [ReadOnly] public int inSeedCount;
        [ReadOnly] public int inConnectionCount;

        public NativeSlice<ByteBounds> inOutBounds;
        public NativeSlice<byte2> inOutConnections;
        
        [WriteOnly] public NativeSlice<byte> outShapeRootSeeds;
        [WriteOnly] public NativeReference<int> outShapeCount;

        private Mask256 _connectionUsageMask;
        private Mask256 _seedUsageMask;
        private Mask256 _currentMask;
        
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
                _currentMask = default;
                _currentMask.SetBitTrue((byte)(seed - 1));
                var connectionFound = true;

                while (connectionFound)
                {
                    connectionFound = false;
                    
                    for (var connectionIndex = 0; connectionIndex < inConnectionCount; connectionIndex++)
                    {
                        if (_connectionUsageMask.HasBit((byte) connectionIndex))
                        {
                            continue;
                        }

                        var connection = inOutConnections[connectionIndex];
                        if (_currentMask.HasBit((byte) (connection.x - 1)))
                        {
                            connectionFound = true;
                            _connectionUsageMask.SetBitTrue((byte) connectionIndex);
                            _seedUsageMask.SetBitTrue((byte) (connection.y - 1));
                            shapeBounds = JoinBounds(shapeBounds, inOutBounds[connection.y]);
                            inOutConnections[connectionIndex] = new byte2(seed, connection.y);
                            _currentMask.SetBitTrue((byte) (connection.y - 1));
                        }
                        else if (_currentMask.HasBit((byte) (connection.y - 1)))
                        {
                            connectionFound = true;
                            _connectionUsageMask.SetBitTrue((byte) connectionIndex);
                            _seedUsageMask.SetBitTrue((byte) (connection.x - 1));
                            shapeBounds = JoinBounds(shapeBounds, inOutBounds[connection.x]);
                            inOutConnections[connectionIndex] = new byte2(seed, connection.x);
                            _currentMask.SetBitTrue((byte) (connection.x - 1));
                        }
                    }
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