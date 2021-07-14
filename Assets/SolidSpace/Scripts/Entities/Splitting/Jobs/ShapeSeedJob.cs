using System;
using System.Runtime.CompilerServices;
using SolidSpace.Entities.Splitting.Enums;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Splitting
{
    [BurstCompile]
    public struct ShapeSeedJob : IJob
    {
        [ReadOnly] public NativeSlice<byte> _inFrameBits;
        [ReadOnly] public int2 _inFrameSize;
        
        public NativeSlice<byte> _outSeedMask;
        public NativeSlice<ByteBounds> _outSeedBounds;
        
        [WriteOnly] public NativeReference<int>  _outSeedCount;
        [WriteOnly] public NativeSlice<byte2> _outConnections;
        [WriteOnly] public NativeReference<int> _outConnectionCount;
        [WriteOnly] public NativeReference<EShapeFillResult> _outResultCode;

        private EShapeFillResult _resultCode;
        private int _seedCount;
        private int _connectionCount;
        
        public void Execute()
        {
            var bytesPerLine = (int) Math.Ceiling(_inFrameSize.x / 8f);
            Mask256 previousFill = default;
            
            _resultCode = EShapeFillResult.Unknown;
            _outResultCode.Value = EShapeFillResult.Unknown;
            _connectionCount = 0;
            _seedCount = 0;
            
            for (var lineIndex = 0; lineIndex < _inFrameSize.y; lineIndex++)
            {
                var frame = ReadMask(_inFrameBits, lineIndex * bytesPerLine, bytesPerLine);
                var frameOffset = lineIndex * _inFrameSize.x;
                for (var i = 0; i < _inFrameSize.x; i++)
                {
                    if (!frame.HasBit((byte) i))
                    {
                        _outSeedMask[frameOffset + i] = 0;
                    }
                }
                
                var newFill = ProjectPreviousFillOnFrame(frame, previousFill, lineIndex, lineIndex - 1);
                if (_resultCode != EShapeFillResult.Unknown)
                {
                    _outResultCode.Value = _resultCode;
                    return;
                }

                ProcessSeeds(frame, newFill, lineIndex);
                if (_resultCode != EShapeFillResult.Unknown)
                {
                    _outResultCode.Value = _resultCode;
                    return;
                }

                previousFill = frame;
            }

            _outResultCode.Value = EShapeFillResult.Normal; 
            _outConnectionCount.Value = _connectionCount;
            _outSeedCount.Value = _seedCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessSeeds(Mask256 frame, Mask256 newFill, int frameY)
        {
            var seeds = frame ^ newFill;
            if (!seeds.HasAnyBit())
            {
                return;
            }
            
            var offset = frameY * _inFrameSize.x;
            for (var i = 0; i < _inFrameSize.x; i++)
            {
                if (!seeds.HasBit((byte) i))
                {
                    continue;
                }

                if (_seedCount >= _outSeedBounds.Length - 1)
                {
                    _resultCode = EShapeFillResult.TooManyShapes;
                    return;
                }

                var shapeId = _seedCount++ + 1;
                ByteBounds shape;
                shape.min.x = (byte) i;
                shape.min.y = (byte) frameY;
                shape.max.y = (byte) frameY;
                _outSeedMask[offset + i] = (byte) shapeId;
                i++;
                while (i < _inFrameSize.x && seeds.HasBit((byte) i))
                {
                    _outSeedMask[offset + i] = (byte) shapeId;
                    i++;
                }

                shape.max.x = (byte) (i - 1);
                _outSeedBounds[shapeId] = shape;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Mask256 ProjectPreviousFillOnFrame(Mask256 frame, Mask256 previousFill, int frameY, int previousFillY)
        {
            Mask256 newFill = default;

            var frameAndPreviousFill = frame & previousFill;
            if (!frameAndPreviousFill.HasAnyBit())
            {
                return newFill;
            }

            var currentFillOffset = frameY * _inFrameSize.x;
            var previousLineOffset = previousFillY * _inFrameSize.x;
            var lastCreatedLineBounds = new byte2(-1, -1);
            var lastCreatedLineId = -1;

            for (var i = 0; i < _inFrameSize.x; i++)
            {
                if (!frameAndPreviousFill.HasBit((byte) i))
                {
                    continue;
                }

                var shapeId = _outSeedMask[previousLineOffset + i];
                
                if (i >= lastCreatedLineBounds.x && i <= lastCreatedLineBounds.y)
                {
                    if (shapeId != lastCreatedLineId)
                    {
                        if (_connectionCount >= _outConnections.Length)
                        {
                            _resultCode = EShapeFillResult.TooManyShapeConnections;
                            return default;
                        }
                        
                        _outConnections[_connectionCount++] = new byte2(shapeId, lastCreatedLineId);
                    }

                    i++;
                    while (i <= lastCreatedLineBounds.y && previousFill.HasBit((byte) i))
                    {
                        i++;
                    }
                    
                    continue;
                }
                
                var shapeInfo = _outSeedBounds[shapeId];
                shapeInfo.max.y = (byte) Math.Max(shapeInfo.max.y, frameY);
                _outSeedMask[currentFillOffset + i] = shapeId;
                newFill.SetBitTrue((byte) i);
                        
                var toStart = i - 1;
                for (; toStart >= 0 && frame.HasBit((byte) toStart); toStart--)
                {
                    _outSeedMask[currentFillOffset + toStart] = shapeId;
                    newFill.SetBitTrue((byte) toStart);
                }
                shapeInfo.min.x = (byte) Math.Min(shapeInfo.min.x, ++toStart);

                var toEnd = i + 1;
                for (; toEnd < _inFrameSize.x && frame.HasBit((byte) toEnd); toEnd++)
                {
                    _outSeedMask[currentFillOffset + toEnd] = shapeId;
                    newFill.SetBitTrue((byte) toEnd);
                }
                shapeInfo.max.x = (byte) Math.Max(shapeInfo.max.x, --toEnd);

                _outSeedBounds[shapeId] = shapeInfo;
                lastCreatedLineBounds = new byte2(toStart, toEnd);
                lastCreatedLineId = shapeId;

                i++;
                while (i <= toEnd && previousFill.HasBit((byte) i))
                {
                    i++;
                }
            }

            return newFill;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly Mask256 ReadMask(NativeSlice<byte> bits, int offset, int count)
        {
            Mask256 resultMask = default;
            
            for (var i = 0; i < count; i++)
            {
                AddBits(ref resultMask, bits[offset + i], i * 8);
            }

            return resultMask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddBits(ref Mask256 mask, byte bits, int offset)
        {
            if (offset < 128)
            {
                if (offset < 64)
                {
                    mask.v0 |= ((long) bits) << offset;
                    return;
                }

                mask.v1 |= ((long) bits) << (offset - 64);
                return;
            }

            if (offset < 192)
            {
                mask.v2 |= ((long) bits) << (offset - 128);
                return;
            }

            mask.v3 |= ((long) bits) << (offset - 192);
        }
    }
}