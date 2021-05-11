using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SolidSpace.Automation.NamespaceTool
{
    internal class FolderScanner
    {
        public void Scan(string projectRoot, Config config, ICollection<EntityInfo> output)
        {
            output.Clear();
            
            ScanRecursive(projectRoot + "/" + config.ScriptsRoot, config.FolderFilters, output);
        }

        private void ScanRecursive(string path, IReadOnlyList<FilterInfo> filters, ICollection<EntityInfo> output)
        {
            for (var i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                if (!filter.enabled)
                {
                    continue;
                }

                if (!Regex.IsMatch(path, filter.regex))
                {
                    continue;
                }

                output.Add(new EntityInfo
                {
                    name = path,
                    regexId = i
                });
                
                break;
            }
            
            var subDirectories = Directory.GetDirectories(path);
            
            foreach (var directory in subDirectories)
            {
                ScanRecursive(path + "/" + Path.GetFileName(directory), filters, output);
            }
        }
    }
}