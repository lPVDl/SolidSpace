using System.IO;
using System.Text.RegularExpressions;
using SolidSpace.DataValidation;

namespace SolidSpace.IO.Editor
{
    [InspectorDataValidator]
    public class EditorFilePathValidator : IDataValidator<EditorFilePath>
    {
        private const string BlacklistRegex = @"(\\)|(\/$)|(^\/)|(\/\/)";
        
        public string Validate(EditorFilePath data)
        {
            if (data.path is null)
            {
                return $"'{nameof(data.path)}' is null";
            }

            var match = Regex.Match(data.path, BlacklistRegex);
            if (match.Success)
            {
                return $"'{nameof(data.path)}' contains|starts|ends with '{match.Value}'";
            }

            var file = EditorPath.Combine(EditorPath.ProjectRoot, data.path);
            if (!File.Exists(file))
            {
                return $"File '{file}' does not exist";
            }
            
            return string.Empty;
        }
    }
}