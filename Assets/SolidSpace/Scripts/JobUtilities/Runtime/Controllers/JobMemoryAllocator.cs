using System;
using System.Collections.Generic;
using Unity.Collections;

namespace SolidSpace.JobUtilities
{
    public class JobMemoryAllocator
    {
        private List<IDisposable> _allocations;
        
        public JobMemoryAllocator()
        {
            _allocations = new List<IDisposable>();
        }

        public void AddAllocation(IDisposable disposable)
        {
            _allocations.Add(disposable);
        }

        public NativeArray<T> CreateNativeArray<T>(int length) where T : unmanaged
        {
            var array = new NativeArray<T>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            _allocations.Add(array);
            return array;
        }

        public NativeReference<T> CreateNativeReference<T>() where T : unmanaged
        {
            var reference = new NativeReference<T>(Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            _allocations.Add(reference);
            return reference;
        }

        public void DisposeAllocations()
        {
            for (var i = 0; i < _allocations.Count; i++)
            {
                _allocations[i].Dispose();
            }
            
            _allocations.Clear();
        }
    }
}