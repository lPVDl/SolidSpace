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
                owner._buildTreeJobStopwatch = new Stopwatch();
                owner._records = new NativeArray<ProfilingRecord>(MaxRecordCount, Allocator.Persistent);
                owner._recordCount = 0;
                owner._nameCount = 1;
                owner._namesActive = new string[MaxRecordCount];
                owner._namesPassive = new string[MaxRecordCount];
                owner._namesPassive[0] = RootNodeName;
                owner._stopwatch = new Stopwatch();
                owner._profilingTree = new ProfilingTree
                {
                    childs = _arrayUtil.CreateTempJobArray<ushort>(1),
                    names = _arrayUtil.CreateTempJobArray<ushort>(1),
                    siblings = _arrayUtil.CreateTempJobArray<ushort>(1),
                    text = owner._namesPassive
                };
                owner._profilingTree.childs[0] = 0;
                owner._profilingTree.names[0] = 0;
                owner._profilingTree.siblings[0] = 0;
            }

            public void Update()
            {
                var temp = owner._namesActive;
                owner._namesActive = owner._namesPassive;
                owner._namesPassive = temp;
                owner._namesActive[0] = RootNodeName;
                owner._nameCount = 1;

                owner._enableSolidProfiling = owner._config.EnableSolidProfiling;
                owner._enableUnityProfiling = owner._config.EnableUnityProfiling;

                owner._profilingTree.childs.Dispose();
                owner._profilingTree.names.Dispose();
                owner._profilingTree.siblings.Dispose();

                var nodeCount = owner._recordCount / 2 + 2;

                var job = new ProfilingBuildTreeJob
                {
                    inRecords = owner._records,
                    inRecordCount = owner._recordCount,
                    outChilds = _arrayUtil.CreateTempJobArray<ushort>(nodeCount),
                    outNames = _arrayUtil.CreateTempJobArray<ushort>(nodeCount),
                    outSiblings = _arrayUtil.CreateTempJobArray<ushort>(nodeCount),
                    parentStack = _arrayUtil.CreateTempJobArray<ushort>(JobStackSize),
                    siblingStack = _arrayUtil.CreateTempJobArray<ushort>(JobStackSize),
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

                owner._profilingTree = new ProfilingTree
                {
                    childs = job.outChilds,
                    names = job.outNames,
                    siblings = job.outSiblings,
                    text = owner._namesPassive
                };
                
                owner._stopwatch.Reset();
                owner._stopwatch.Start();
            }

            public void FinalizeObject()
            {
                owner._records.Dispose();
                owner._profilingTree.childs.Dispose();
                owner._profilingTree.names.Dispose();
                owner._profilingTree.siblings.Dispose();
            }
        }
    }
}