using System;
using System.Text.RegularExpressions;

namespace SolidSpace.Editor.CodeInspection.NamespaceTool
{
    public class NamespaceToolFolderFilterValidator : IDataValidator<NamespaceToolFilter>
    {
        public string Validate(NamespaceToolFilter data)
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