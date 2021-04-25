using System.Runtime.CompilerServices;

namespace SolidSpace.Profiling.Data
{
    public struct ProfilingRecord
    {
        private const uint TimeSamplesMask = 0x7FFFFFFF;
        private const uint CommandTypeMask = 0x80000000;
        
        private uint _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int timeSamples, bool isBeginSampleCommand)
        {
            _value = (uint) (timeSamples & TimeSamplesMask) | (isBeginSampleCommand ? CommandTypeMask : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out int timeSamples, out bool isBeginSampleCommand)
        {
            isBeginSampleCommand = (_value & CommandTypeMask) != 0;
            timeSamples = (int) (_value & TimeSamplesMask);
        }
    }
}