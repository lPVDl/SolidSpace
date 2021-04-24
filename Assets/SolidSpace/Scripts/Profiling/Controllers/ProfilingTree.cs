using System;
using System.Collections.Generic;
using SolidSpace.Profiling.Data;
using SolidSpace.Profiling.Interfaces;

namespace SolidSpace.Profiling.Controllers
{
    public class ProfilingTree : IProfilingTree
    {
        private const int MaxNodeCount = 1 << 16;

        public IReadOnlyList<ProfilingNode> Nodes => _nodes;
        
        public IReadOnlyList<string> Names => _names;
        
        private List<ProfilingNode> _nodes;
        private Stack<ushort> _parentStack;
        private Stack<ushort> _siblingStack;
        private Dictionary<string, int> _nameToIndex;
        private List<string> _names;
        private int _nextFreeName;

        public ProfilingTree()
        {
            _siblingStack = new Stack<ushort>();
            _parentStack = new Stack<ushort>();
            _nodes = new List<ProfilingNode>();
            _nameToIndex = new Dictionary<string, int>();
            _names = new List<string>();
        }

        public void Clear()
        {
            _nodes.Clear();
            _parentStack.Clear();
            _nameToIndex.Clear();
            _siblingStack.Clear();
            _names.Clear();
            
            _siblingStack.Push(0);
            _parentStack.Push(0);
            _nodes.Add(new ProfilingNode
            {
                name = 0,
                child = 0,
                sibling = 0,
            });
            _nextFreeName = 1;
            _nameToIndex["_root"] = 0;
            _names.Add("_root");
        }

        public void BeginSample(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            
            var nodeCount = _nodes.Count;
            if (nodeCount >= MaxNodeCount)
            {
                throw new InvalidOperationException($"Exceed max node count: {MaxNodeCount}");
            }

            if (!_nameToIndex.TryGetValue(name, out var nameIndex))
            {
                nameIndex = _nextFreeName++;
                _nameToIndex[name] = nameIndex;
                _names.Add(name);
            }

            var nodeIndex = (ushort) nodeCount;
            var siblingIndex = _siblingStack.Pop();

            if (siblingIndex == 0)
            {
                var parentIndex = _parentStack.Peek();
                var parentNode = _nodes[parentIndex];
                parentNode.child = nodeIndex;
                _nodes[parentIndex] = parentNode;
            }
            else
            {
                var siblingNode = _nodes[siblingIndex];
                siblingNode.sibling = nodeIndex;
                _nodes[siblingIndex] = siblingNode;
            }
            
            _siblingStack.Push(nodeIndex);
            _siblingStack.Push(0);
            _parentStack.Push(nodeIndex);

            _nodes.Add(new ProfilingNode
            {
                name = (ushort) nameIndex,
                child = 0,
                sibling = 0,
            });
        }

        public void EndSample()
        {
            if (_parentStack.Count == 1)
            {
                throw new InvalidOperationException("EndSample() was called without StartSample()");
            }

            _parentStack.Pop();
            _siblingStack.Pop();
        }
    }
}