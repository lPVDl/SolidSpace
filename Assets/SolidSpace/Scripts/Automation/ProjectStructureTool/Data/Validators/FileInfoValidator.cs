using System;
using System.Text.RegularExpressions;
using SolidSpace.DataValidation;

namespace SolidSpace.Automation.ProjectStructureTool
{
    internal class FileInfoValidator : IDataValidator<FilterInfo>
    {
        public string Validate(FilterInfo data)
        {
            if (data.regex is null)
            {
                return $"{nameof(data.regex)} is null";
            }

            try
            {
                Regex.IsMatch(string.Empty, data.regex);
            }
            catch (Exception e)
            {
                return $"{nameof(data.regex)} is invalid: {e.Message}";
            }

            return string.Empty;
        }
    }
}