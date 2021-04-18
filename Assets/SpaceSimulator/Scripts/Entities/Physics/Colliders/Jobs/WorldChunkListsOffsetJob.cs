using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SpaceSimulator.Entities.Physics
{
    [BurstCompile]
    public struct WorldChunkListsOffsetJob : IJob
    {
        [ReadOnly] public int inListCount;
        
        public NativeArray<ColliderListPointer> inOutLists;
        
        public void Execute()
        {
            var offset = 0;
            for (var i = 0; i < inListCount; i++)
            {
                var listPointer = inOutLists[i];
                if (listPointer.count == 0)
                {
                    continue;
                }

                listPointer.offset = offset;
                offset += listPointer.count;
                inOutLists[i] = listPointer;
            }
        }
    }
}