using System;
using System.Collections.Generic;
using Unity.Collections;

namespace SolidSpace.Profiling
{
    public partial class ProfilingManager
    {
        private struct ExceptionHandler
        {
            public void HandleJobState(IList<string> names, ProfilingBuildTreeJob job)
            {
                var state = job.outState[0];

                if (state.code == EProfilingBuildTreeCode.Success)
                {
                    return;
                }
                
                var stackTrace = BuildPath(names, job.parentStack, state.stackLast);
                switch (state.code)
                {
                    case EProfilingBuildTreeCode.StackIsNotEmptyAfterJobComplete:
                        throw new InvalidOperationException($"EndSample() was not called for ${stackTrace}");
                    
                    case EProfilingBuildTreeCode.StackOverflow:
                        throw new StackOverflowException($"{stackTrace}{names[state.recordLast + 1]} caused stack overflow");
                }

                throw new InvalidOperationException($"Error during tree building: {state.code}");
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