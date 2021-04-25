using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace SolidSpace
{
    public struct NativeArrayUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> CreateTempJobArray<T>(int length) where T : struct
        {
            return new NativeArray<T>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> CreatePersistentArray<T>(int length) where T : struct
        {
            return new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MaintainPersistentArrayLength<T>(ref NativeArray<T> array, int requiredCapacity, int chunkSize) 
            where T : struct
        {
            if (array.Length >= requiredCapacity)
            {
                return;
            }
            
            var chunkBasedLength = (int) math.ceil(requiredCapacity / (float) chunkSize) * chunkSize;
            array.Dispose();
            array = CreatePersistentArray<T>(chunkBasedLength);
        }
    }
}