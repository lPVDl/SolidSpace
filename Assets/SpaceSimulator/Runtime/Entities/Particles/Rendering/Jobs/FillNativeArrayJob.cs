using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SpaceSimulator.Runtime.Entities.Particles.Rendering
{
    [BurstCompile]
    public struct FillNativeArrayJob<T> : IJobParallelFor where T : struct
    {
        [WriteOnly][NativeDisableParallelForRestriction] public NativeArray<T> array;
        [ReadOnly] public int offset;
        [ReadOnly] public T value;

        public void Execute(int index)
        {
            array[index + offset] = value;
        }
    }
}