using System;
using System.Collections.Generic;
using System.Linq;
using SolidSpace.Profiling.Enums;
using Unity.Collections;

namespace SolidSpace.Profiling
{
    public partial class ProfilingManager
    {
        private struct ErrorHandler
        {
            public void HandleJobState(IList<string> names, ProfilingBuildTreeJob job)
            {
                var status = job.outStatus[0];

                if (status == EProfilingBuildTreeStatus.Success)
                {
                    return;
                }
                
                var stackTrace = BuildPath(names, job.parentStack, job.outStackLast[0]);
                if (status == EProfilingBuildTreeStatus.StackIsNotEmptyAfterJobComplete)
                {
                    throw new InvalidOperationException($"EndSample was not called for ${stackTrace}");
                }
                
                throw new InvalidOperationException($"Error during tree building: '{job.outStatus[0]}'");
            }
            
            private string BuildPath(IList<string> names, NativeArray<ushort> stack, int stackLast)
            {
                var result = string.Empty;

                for (var i = 0; i <= stackLast; i++)
                {
                    result += names[stack[i]] + "/";
                }

                return result;
            }
        }
    }
}