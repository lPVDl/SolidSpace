using System;
using System.Text.RegularExpressions;

namespace SpaceSimulator.Editor.CodeInspection.NamespaceTool
{
    public class NamespaceToolFolderFilterValidator : IValidator<NamespaceToolFolderFilter>
    {
        public string Validate(NamespaceToolFolderFilter data)
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