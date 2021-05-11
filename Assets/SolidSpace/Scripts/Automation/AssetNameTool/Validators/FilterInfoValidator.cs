using System;
using System.Text.RegularExpressions;
using SolidSpace.DataValidation;

namespace SolidSpace.Automation.AssetNameTool
{
    internal class FilterInfoValidator : IDataValidator<FilterInfo>
    {
        public string Validate(FilterInfo data)
        {
            if (data.scannerRegex is null || data.nameRegex is null || data.nameSubstitution is null)
            {
                return string.Empty;
            }

            try
            {
                Regex.IsMatch("", data.scannerRegex);
            }
            catch (Exception e)
            {
                return $"'{nameof(data.scannerRegex)}' is invalid: {e.Message}";
            }
            
            try
            {
                Regex.IsMatch("", data.nameRegex);
            }
            catch (Exception e)
            {
                return $"'{nameof(data.nameRegex)}' is invalid: {e.Message}";
            }

            return string.Empty;
        }
    }
}