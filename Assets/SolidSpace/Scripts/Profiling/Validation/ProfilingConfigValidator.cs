namespace SolidSpace.Profiling
{
    public class ProfilingConfigValidator : IValidator<ProfilingConfig>
    {
        private const int MaxRecordCount = (1 << 16) - 2;
        
        public string Validate(ProfilingConfig data)
        {
            if (data.MaxRecordCount < 0 || data.MaxRecordCount > MaxRecordCount)
            {
                return $"{nameof(data.MaxRecordCount)} must be in range [0, {MaxRecordCount}]";
            }

            return string.Empty;
        }
    }
}