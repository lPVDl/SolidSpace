using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Profiling
{
    [BurstCompile]
    public struct ProfilingBuildTreeJob : IJob
    {
        [ReadOnly] public NativeArray<ProfilingRecord> inRecords;
        [ReadOnly] public int inRecordCount;
        [ReadOnly] public long inFrequency;

        public NativeArray<ushort> parentStack;
        public NativeArray<ushort> siblingStack;
        public NativeArray<int> timeStack;

        public NativeArray<ushort> outChilds;
        public NativeArray<ushort> outSiblings;
        public NativeArray<ushort> outNames;
        public NativeArray<float> outTimes;

        private int _stackLast;
        private int _nodeCount;
        private int _stackMax;
        private long _ticksTotal;
        private float _ticksToMilliseconds;

        public void Execute()
        {
            _ticksToMilliseconds = 1000f / inFrequency;
            _stackLast = 0;
            _nodeCount = 1;
            parentStack[0] = 0;
            siblingStack[0] = 0;
            outChilds[0] = 0;
            outNames[0] = 0;
            outSiblings[0] = 0;
            outTimes[0] = 0;
            _stackMax = math.min(parentStack.Length, siblingStack.Length);

            var nameIndex = 1;
            
            for (var i = 0; i < inRecordCount; i++)
            {
                var record = inRecords[i];
                
                record.Read(out var timeSamples, out var isBeginSampleCommand);
                
                if (isBeginSampleCommand)
                {
                    FlushBeginSample(nameIndex++, timeSamples);
                }
                else
                {
                    FlushEndSample(timeSamples);
                }
            }

            outTimes[0] = _ticksTotal * _ticksToMilliseconds;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushBeginSample(int nameIndex, int timeSamples)
        {
            if (_stackLast + 1 >= _stackMax)
            {
                throw new StackOverflowException();
            }
            
            var nodeIndex =  (ushort) _nodeCount++;
            var siblingIndex = siblingStack[_stackLast];

            if (siblingIndex == 0)
            {
                var parentIndex = parentStack[_stackLast];
                outChilds[parentIndex] = nodeIndex;
            }
            else
            {
                outSiblings[siblingIndex] = nodeIndex;
            }

            siblingStack[_stackLast++] = nodeIndex;
            siblingStack[_stackLast] = 0;
            parentStack[_stackLast] = nodeIndex;
            timeStack[_stackLast] = timeSamples;

            outNames[nodeIndex] = (ushort) nameIndex;
            outChilds[nodeIndex] = 0;
            outSiblings[nodeIndex] = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushEndSample(int timeSamples)
        {
            if (_stackLast == 0)
            {
                throw new InvalidOperationException("EndSample() was called without StartSample()");
            }

            var nodeIndex = parentStack[_stackLast];
            var ticks = timeSamples - timeStack[_stackLast];

            if (outChilds[nodeIndex] == 0)
            {
                _ticksTotal += ticks;
            }

            outTimes[nodeIndex] = ticks * _ticksToMilliseconds;
            
            _stackLast--;
        }
    }
}