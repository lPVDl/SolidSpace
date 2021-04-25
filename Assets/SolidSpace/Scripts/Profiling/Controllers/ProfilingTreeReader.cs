using System;
using System.Collections.Generic;
using SolidSpace.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Profiling
{
    public struct ProfilingTreeReader
    {
        private ProfilingTree _tree;
        private NativeArrayUtil _arrayUtil;

        public ProfilingTreeReader(ProfilingTree tree)
        {
            _tree = tree;
        }
        
        public void Read(int offset, int count, List<ProfilingNodeFriendly> result, out int totalNodeCount)
        {
            if (offset < 0) throw new ArgumentException($"{nameof(offset)} can not be negative");
            if (count < 0) throw new ArgumentException($"{nameof(count)} can not be negative");
            if (result is null) throw new ArgumentNullException(nameof(result));

            var job = new ProfilingReadTreeJob
            {
                inChilds = _tree.childs,
                inNames = _tree.names,
                inSiblings = _tree.siblings,
                inReadRange = new int2(offset, offset + count - 1),
                outNodeCount = _arrayUtil.CreateTempJobArray<int>(1),
                outTotalNodeCount = _arrayUtil.CreateTempJobArray<int>(1),
                outNodes = _arrayUtil.CreateTempJobArray<ProfilingNode>(count)
            };
            job.Schedule().Complete();
            
            var nodeCount = job.outNodeCount[0];
            result.Clear();
            
            for (var i = 0; i < nodeCount; i++)
            {
                var node = job.outNodes[i];
                
                result.Add(new ProfilingNodeFriendly
                {
                    name = _tree.text[node.name],
                    deep = node.deep
                });
            }

            totalNodeCount = job.outTotalNodeCount[0];

            job.outNodes.Dispose();
            job.outNodeCount.Dispose();
            job.outTotalNodeCount.Dispose();
        }
    }
}