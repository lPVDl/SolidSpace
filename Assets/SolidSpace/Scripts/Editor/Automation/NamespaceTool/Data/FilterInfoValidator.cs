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
                return $"{nameof(data.regex)} is null";
            }

            if (!ValidationUtil.RegexIsValid(nameof(data.regex), data.regex, out var message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}