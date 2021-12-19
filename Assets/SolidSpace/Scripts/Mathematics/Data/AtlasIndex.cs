using System.Runtime.CompilerServices;

namespace SolidSpace.Mathematics
{
    public struct AtlasIndex
    {
        private ushort _value;

        public AtlasIndex(int chunkId, int itemId)
        {
            _value = (ushort) (((chunkId & 0xFFF) << 4) + (itemId & 0xF));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out int chunkId, out int itemId)
        {
            chunkId = ReadChunkId();
            itemId = ReadItemId();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadChunkId()
        {
            return (_value & 0xFFF0) >> 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadItemId()
        {
            return _value & 0xF;
        }

        public override string ToString()
        {
            Read(out var chunkId, out var itemId);
            
            return $"(chunkId: {chunkId}; itemId: {itemId})";
        }
    }
}