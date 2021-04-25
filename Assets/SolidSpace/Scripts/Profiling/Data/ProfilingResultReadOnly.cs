using System.Runtime.CompilerServices;

namespace SolidSpace.Profiling.Data
{
    public readonly struct ProfilingResultReadOnly
    {
        private readonly ProfilingResult _tree;
        
        public ProfilingResultReadOnly(ProfilingResult tree)
        {
            _tree = tree;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNodeChild(int index)
        {
            return _tree.childIndexes[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNodeSibling(int index)
        {
            return _tree.siblingIndexes[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetNodeName(int index)
        {
            return _tree.names[index];
        }
    }
}