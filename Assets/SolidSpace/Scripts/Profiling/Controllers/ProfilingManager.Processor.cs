using System.Collections.Generic;
using SolidSpace.Entities;
using SolidSpace.Profiling.Data;
using SolidSpace.Profiling.Jobs;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SolidSpace.Profiling.Controllers
{
    public partial class ProfilingManager
    {
        private struct Processor
        {
            public ProfilingManager owner;

            // TODO: Move array util to SolidSpace.Common
            private NativeArrayUtil _arrayUtil;

            public void Initialize()
            {
                owner._records = new NativeArray<ProfilingRecord>(MaxRecordCount, Allocator.Persistent);
                owner._recordCount = 0;
                owner._namesActive = new List<string>();
                owner._namesPassive = new List<string>();
                owner._namesPassive.Add(RootNodeName);
                owner._profilingResult = new ProfilingResult
                {
                    childIndexes = _arrayUtil.CreateTempJobArray<ushort>(1),
                    nameIndexes = _arrayUtil.CreateTempJobArray<ushort>(1),
                    siblingIndexes = _arrayUtil.CreateTempJobArray<ushort>(1),
                    names = owner._namesPassive
                };
                owner._profilingResult.childIndexes[0] = 0;
                owner._profilingResult.nameIndexes[0] = 0;
                owner._profilingResult.siblingIndexes[0] = 0;
                
                Instance = owner;
            }

            public void Update()
            {
                var temp = owner._namesActive;
                owner._namesActive = owner._namesPassive;
                owner._namesPassive = temp;
                owner._namesActive.Clear();
                owner._namesActive.Add(RootNodeName);

                owner._enableSolidProfiling = owner._config.EnableSolidProfiling;
                owner._enableUnityProfiling = owner._config.EnableUnityProfiling;
                owner._frameStartTime = Time.realtimeSinceStartupAsDouble;

                owner._profilingResult.childIndexes.Dispose();
                owner._profilingResult.nameIndexes.Dispose();
                owner._profilingResult.siblingIndexes.Dispose();

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
                
                Profiler.BeginSample("ProfilingManager.BuildTreeJob");
                job.Schedule().Complete();
                Profiler.EndSample();
                
                owner._recordCount = 0;

                job.parentStack.Dispose();
                job.siblingStack.Dispose();

                owner._profilingResult = new ProfilingResult
                {
                    childIndexes = job.outChilds,
                    nameIndexes = job.outNames,
                    siblingIndexes = job.outSiblings,
                    names = owner._namesPassive
                };
            }

            public void FinalizeObject()
            {
                owner._records.Dispose();
                owner._profilingResult.childIndexes.Dispose();
                owner._profilingResult.nameIndexes.Dispose();
                owner._profilingResult.siblingIndexes.Dispose();
                
                Instance = null;
            }
        }
    }
}