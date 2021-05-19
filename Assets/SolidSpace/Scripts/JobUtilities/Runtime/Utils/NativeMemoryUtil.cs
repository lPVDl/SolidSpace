using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace SolidSpace.JobUtilities
{
    public static class NativeMemoryUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreateTempJobArray<T>(int length) where T : struct
        {
            return new NativeArray<T>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreatePersistentArray<T>(int length) where T : struct
        {
            return new NativeArray<T>(length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeReference<T> CreatePersistentReference<T>(T withValue) where T : unmanaged
        {
            var reference = new NativeReference<T>(Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            reference.Value = withValue;
            return reference;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MaintainPersistentArrayLength<T>(ref NativeArray<T> array, int requiredCapacity, int chunkSize) 
            where T : struct
        {
            if (array.Length >= requiredCapacity)
            {
                return;
            }
            
            var chunkBasedLength = (int) Math.Ceiling(requiredCapacity / (float) chunkSize) * chunkSize;
            array.Dispose();
            array = CreatePersistentArray<T>(chunkBasedLength);
        }
    }
}