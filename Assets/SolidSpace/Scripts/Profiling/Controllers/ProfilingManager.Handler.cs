using System.Runtime.CompilerServices;
using SolidSpace.Profiling.Data;
using UnityEngine;
using UnityEngine.Profiling;

namespace SolidSpace.Profiling.Controllers
{
    public partial class ProfilingManager
    {
        private struct Handler
        {
            public ProfilingManager owner;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnBeginSample(string name)
            {
                if (owner._enableUnityProfiling)
                {
                    Profiler.BeginSample(name);
                }

                if (!owner._enableSolidProfiling || owner._recordCount >= MaxRecordCount)
                {
                    return;
                }

                var record = new ProfilingRecord();
                record.Write(GetCurrentTimeSamples(), true);
                owner._records[owner._recordCount++] = record;
                owner._namesActive.Add(name);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnEndSample(string name)
            {
                if (owner._enableUnityProfiling)
                {
                    Profiler.EndSample();
                }
            
                if (!owner._enableSolidProfiling || owner._recordCount >= MaxRecordCount)
                {
                    return;
                }

                var record = new ProfilingRecord();
                record.Write(GetCurrentTimeSamples(), false);
                owner._records[owner._recordCount++] = record;
                owner._namesActive.Add(name);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private int GetCurrentTimeSamples()
            {
                // TODO: Use StopWatch.elapsed ticks
                return (int) (Time.realtimeSinceStartupAsDouble - owner._frameStartTime) / SamplesPerSecond;
            }
        }
    }
}