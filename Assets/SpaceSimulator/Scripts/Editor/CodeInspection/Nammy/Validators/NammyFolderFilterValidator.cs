using System;
using System.Text.RegularExpressions;

namespace SpaceSimulator.Editor.CodeInspection.Nammy
{
    public class NammyFolderFilterValidator : IValidator<NammyFolderFilter>
    {
        public string Validate(NammyFolderFilter data)
        {
            if (data.regex is null)
            {
                return $"'{nameof(data.regex)}' is null";
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