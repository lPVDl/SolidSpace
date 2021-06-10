using System;
using System.Text.RegularExpressions;
using SolidSpace.DataValidation;

namespace SolidSpace.Editor.Automation.NamespaceTool
{
    [InspectorDataValidator]
    internal class FilterInfoValidator : IDataValidator<FilterInfo>
    {
        public string Validate(FilterInfo data)
        {
            if (data.regex is null)
            {
                return string.Empty;
            }

            try
            {
                Regex.IsMatch("", data.regex);
            }
            catch (Exception e)
            {
                return $"'{nameof(data.regex)}' is invalid: {e.Message}";
            }

            return string.Empty;
        }
    }
}