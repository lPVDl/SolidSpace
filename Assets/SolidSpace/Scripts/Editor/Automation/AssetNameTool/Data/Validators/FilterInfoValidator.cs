using System;
using System.Text.RegularExpressions;
using SolidSpace.DataValidation;

namespace SolidSpace.Editor.Automation.AssetNameTool.Validators
{
    [InspectorDataValidator]
    internal class FilterInfoValidator : IDataValidator<FilterInfo>
    {
        public string Validate(FilterInfo data)
        {
            if (data.scannerRegex is null)
            {
                return $"{nameof(data.scannerRegex)} is null";
            }

            if (data.nameRegex is null)
            {
                return $"{nameof(data.nameRegex)} is null";
            }

            if (data.nameSubstitution is null)
            {
                return $"{nameof(data.nameSubstitution)} is null";
            }

            if (!ValidationUtil.RegexIsValid(nameof(data.scannerRegex), data.scannerRegex, out var message))
            {
                return message;
            }

            if (!ValidationUtil.RegexIsValid(nameof(data.nameRegex), data.nameRegex, out message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}