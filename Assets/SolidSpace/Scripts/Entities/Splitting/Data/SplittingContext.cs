using SolidSpace.JobUtilities;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Splitting
{
    public struct SplittingContext
    {
        public ESplittingState state;
        public Entity entity;
        public JobMemoryAllocator jobMemory;

        public int jobDifficulty;
        public JobHandle jobHandle;
        public ShapeSeedJob seedJob;
        public ShapeReadJob readJob;
    }
}