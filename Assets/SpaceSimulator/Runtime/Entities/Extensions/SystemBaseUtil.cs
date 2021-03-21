using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace SpaceSimulator.Runtime.Entities.Extensions
{
    public struct SystemBaseUtil
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
            var chunkBasedLength = Mathf.CeilToInt(requiredCapacity / (float) chunkSize) * chunkSize;
            if (array.Length < chunkBasedLength)
            {
                array.Dispose();
                array = CreatePersistentArray<T>(chunkBasedLength);
            }
        }
    }
}