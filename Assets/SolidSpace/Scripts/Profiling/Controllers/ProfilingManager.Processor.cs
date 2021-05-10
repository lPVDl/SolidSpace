using System.Diagnostics;
using SolidSpace.DebugUtils;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Profiling;

namespace SolidSpace.Profiling
{
    public partial class ProfilingManager
    {
        private struct Processor
        {
            public ProfilingManager owner;

            private NativeArrayUtil _arrayUtil;

            public void Initialize()
            {
                owner._enableSolidProfiling = owner._config.EnableSolidProfiling;
                owner._enableUnityProfiling = owner._config.EnableUnityProfiling;
                owner._buildTreeJobStopwatch = new Stopwatch();
                owner._records = new NativeArray<ProfilingRecord>(MaxRecordCount, Allocator.Persistent);
                owner._recordCount = 0;
                owner._nameCount = 1;
                owner._namesActive = new string[MaxRecordCount];
                owner._namesActive[0] = RootNodeName;
                owner._namesPassive = new string[MaxRecordCount];
                owner._namesPassive[0] = RootNodeName;
                owner._stopwatch = new Stopwatch();
                owner._profilingTree = new ProfilingTree
                {
                    childs = _arrayUtil.CreateTempJobArray<ushort>(1),
                    names = _arrayUtil.CreateTempJobArray<ushort>(1),
                    siblings = _arrayUtil.CreateTempJobArray<ushort>(1),
                    times = _arrayUtil.CreateTempJobArray<float>(1),
                    text = owner._namesActive
                };
                owner._profilingTree.times[0] = 0;
                owner._profilingTree.childs[0] = 0;
                owner._profilingTree.names[0] = 0;
                owner._profilingTree.siblings[0] = 0;
            }

            public void Update()
            {
                owner._enableSolidProfiling = owner._config.EnableSolidProfiling;
                owner._enableUnityProfiling = owner._config.EnableUnityProfiling;

                owner._profilingTree.Dispose();

                var nodeCount = owner._recordCount / 2 + 2;

                var job = new ProfilingBuildTreeJob
                {
                    inRecords = owner._records,
                    inRecordCount = owner._recordCount,
                    inFrequency = Stopwatch.Frequency,
                    outChilds = _arrayUtil.CreateTempJobArray<ushort>(nodeCount),
                    outNames = _arrayUtil.CreateTempJobArray<ushort>(nodeCount),
                    outSiblings = _arrayUtil.CreateTempJobArray<ushort>(nodeCount),
                    outTimes = _arrayUtil.CreateTempJobArray<float>(nodeCount),
                    parentStack = _arrayUtil.CreateTempJobArray<ushort>(JobStackSize),
                    siblingStack = _arrayUtil.CreateTempJobArray<ushort>(JobStackSize),
                    timeStack = _arrayUtil.CreateTempJobArray<int>(JobStackSize)
                };

                var timer = owner._buildTreeJobStopwatch;
                Profiler.BeginSample("ProfilingManager.BuildTreeJob");
                timer.Reset();
                timer.Start();
                job.Schedule().Complete();
                timer.Stop();
                Profiler.EndSample();
                
                SpaceDebug.LogState("BuildTreeJob ms", timer.ElapsedTicks / (float) Stopwatch.Frequency * 1000);
                
                owner._recordCount = 0;

                job.parentStack.Dispose();
                job.siblingStack.Dispose();
                job.timeStack.Dispose();

                owner._profilingTree = new ProfilingTree
                {
                    childs = job.outChilds,
                    names = job.outNames,
                    siblings = job.outSiblings,
                    times = job.outTimes,
                    text = owner._namesActive
                };
                
                Swap(ref owner._namesActive, ref owner._namesPassive);
                owner._nameCount = 1;
                
                owner._stopwatch.Reset();
                owner._stopwatch.Start();
            }

            private static void Swap<T>(ref T a, ref T b)
            {
                var t = a;
                a = b;
                b = t;
            }

            public void FinalizeObject()
            {
                owner._records.Dispose();
                owner._profilingTree.Dispose();
            }
        }
    }
}