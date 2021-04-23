using System;
using System.Text.RegularExpressions;

namespace SpaceSimulator.Editor.Serialization.AssetNameTool.Validators
{
    public class AssetNameToolFolderValidator : IValidator<AssetNameToolFolderFilter>
    {
        public string Validate(AssetNameToolFolderFilter data)
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