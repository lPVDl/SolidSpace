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
    public struct ShapeFillJob : IJob
    {
        public NativeSlice<byte> _inFrameBits;
        public int2 _inFrameSize;
        
        public NativeSlice<byte> _outShapeConnections;
        public NativeReference<int> _outShapeConnectionCount;
        public NativeSlice<byte> _outShapesMask;
        public NativeSlice<ShapeInfo> _outShapesInfo;
        public NativeReference<int>  _outShapeCount;
        public NativeReference<EShapeFillResult> _outResultCode;

        private EShapeFillResult _resultCode;
        private int _seedCount;
        private int _shapeConnectionCount;
        
        public void Execute()
        {
            var bytesPerLine = (int) Math.Ceiling(_inFrameSize.x / 8f);
            Mask256 previousFill = default;
            
            _resultCode = EShapeFillResult.Unknown;
            _outResultCode.Value = EShapeFillResult.Unknown;
            _shapeConnectionCount = 0;
            _seedCount = 0;
            
            for (var lineIndex = 0; lineIndex < _inFrameSize.y; lineIndex++)
            {
                var frame = ReadMask(_inFrameBits, lineIndex * bytesPerLine, bytesPerLine);
                var frameOffset = lineIndex * _inFrameSize.x;
                for (var i = 0; i < _inFrameSize.x; i++)
                {
                    if (!frame.HasBit((byte) i))
                    {
                        _outShapesMask[frameOffset + i] = 0;
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
            _outShapeConnectionCount.Value = _shapeConnectionCount;
            _outShapeCount.Value = _seedCount;
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

                if (_seedCount >= _outShapesInfo.Length - 1)
                {
                    _resultCode = EShapeFillResult.TooManyShapes;
                    return;
                }

                var shapeId = _seedCount++ + 1;
                ShapeInfo shape;
                shape.boundsMin.x = (byte) i;
                shape.boundsMin.y = (byte) frameY;
                shape.boundsMax.y = (byte) frameY;
                _outShapesMask[offset + i] = (byte) shapeId;
                i++;
                while (i < _inFrameSize.x && seeds.HasBit((byte) i))
                {
                    _outShapesMask[offset + i] = (byte) shapeId;
                    i++;
                }

                shape.boundsMax.x = (byte) (i - 1);
                _outShapesInfo[shapeId] = shape;
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

                var shapeId = _outShapesMask[previousLineOffset + i];
                
                if (i >= lastCreatedLineBounds.x && i <= lastCreatedLineBounds.y)
                {
                    if (shapeId != lastCreatedLineId)
                    {
                        if (_shapeConnectionCount >= _outShapeConnections.Length)
                        {
                            _resultCode = EShapeFillResult.TooManyShapeConnections;
                            return default;
                        }
                        
                        _outShapeConnections[_shapeConnectionCount++] = (byte)(shapeId | lastCreatedLineId);
                    }

                    i++;
                    while (i <= lastCreatedLineBounds.y && previousFill.HasBit((byte) i))
                    {
                        i++;
                    }
                    
                    continue;
                }
                
                var shapeInfo = _outShapesInfo[shapeId];
                shapeInfo.boundsMax.y = (byte) Math.Max(shapeInfo.boundsMax.y, frameY);
                _outShapesMask[currentFillOffset + i] = shapeId;
                newFill.SetBitTrue((byte) i);
                        
                var toStart = i - 1;
                for (; toStart >= 0 && frame.HasBit((byte) toStart); toStart--)
                {
                    _outShapesMask[currentFillOffset + toStart] = shapeId;
                    newFill.SetBitTrue((byte) toStart);
                }
                shapeInfo.boundsMin.x = (byte) Math.Min(shapeInfo.boundsMin.x, ++toStart);

                var toEnd = i + 1;
                for (; toEnd < _inFrameSize.x && frame.HasBit((byte) toEnd); toEnd++)
                {
                    _outShapesMask[currentFillOffset + toEnd] = shapeId;
                    newFill.SetBitTrue((byte) toEnd);
                }
                shapeInfo.boundsMax.x = (byte) Math.Max(shapeInfo.boundsMax.x, --toEnd);

                _outShapesInfo[shapeId] = shapeInfo;
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