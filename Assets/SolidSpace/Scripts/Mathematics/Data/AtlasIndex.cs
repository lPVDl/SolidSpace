namespace SolidSpace.Mathematics
{
    public struct AtlasIndex
    {
        public ushort chunkId;
        public byte itemId;

        public override string ToString()
        {
            return $"(chunk: {chunkId}; item: {itemId})";
        }
    }
}