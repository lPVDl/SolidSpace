using System;
using System.Runtime.CompilerServices;
using SolidSpace.Profiling.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Profiling.Jobs
{
    [BurstCompile]
    public struct ProfilingBuildTreeJob : IJob
    {
        [ReadOnly] public NativeArray<ProfilingRecord> inRecords;
        [ReadOnly] public int inRecordCount;

        public NativeArray<ushort> parentStack;
        public NativeArray<ushort> siblingStack;

        public NativeArray<ushort> outChilds;
        public NativeArray<ushort> outSiblings;
        public NativeArray<ushort> outNames;

        private int _stackLast;
        private int _nodeCount;
        private int _stackMax;

        public void Execute()
        {
            _stackLast = 0;
            _nodeCount = 1;
            parentStack[0] = 0;
            siblingStack[0] = 0;
            outChilds[0] = 0;
            outNames[0] = 0;
            outSiblings[0] = 0;
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
                    FlushEndSample();
                }
            }
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

            outNames[nodeIndex] = (ushort) nameIndex;
            outChilds[nodeIndex] = 0;
            outSiblings[nodeIndex] = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushEndSample()
        {
            if (_stackLast == 0)
            {
                throw new InvalidOperationException("EndSample() was called without StartSample()");
            }

            _stackLast--;
        }
    }
}