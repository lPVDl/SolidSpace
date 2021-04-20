using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SpaceSimulator.Editor.CodeInspection.Nammy
{
    public class NammyFolderScanner
    {
        public void Scan(string projectRoot, NammyConfig config, ICollection<NammyFolderInfo> output)
        {
            output.Clear();
            
            ScanRecursive(projectRoot + "/" + config.ScriptsRoot, config.FolderFilters, output);
        }

        private void ScanRecursive(string path, IReadOnlyList<NammyFolderFilter> filters, ICollection<NammyFolderInfo> output)
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

                output.Add(new NammyFolderInfo
                {
                    fullPath = path,
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