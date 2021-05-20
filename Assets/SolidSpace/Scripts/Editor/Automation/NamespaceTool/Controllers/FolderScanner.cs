using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SolidSpace.Editor.Automation.NamespaceTool
{
    internal class FolderScanner
    {
        private HashSet<FolderInfo> _outFolders;
        private IReadOnlyList<FilterInfo> _filters;
        private int _rootLength;
        
        public HashSet<FolderInfo> Scan(string projectRoot, Config config)
        {
            _outFolders = new HashSet<FolderInfo>();
            _filters = config.FolderFilters;
            _rootLength = projectRoot.Length + 1;
            
            ScanRecursive(Path.Combine(projectRoot + "/" + config.ScriptsRoot));

            return _outFolders;
        }

        private void ScanRecursive(string path)
        {
            for (var i = 0; i < _filters.Count; i++)
            {
                var filter = _filters[i];
                if (!filter.enabled)
                {
                    continue;
                }

                if (!Regex.IsMatch(path, filter.regex))
                {
                    continue;
                }

                _outFolders.Add(new FolderInfo
                {
                    name = path.Substring(_rootLength),
                    regexId = i
                });
                
                break;
            }
            
            var subDirectories = Directory.GetDirectories(path);
            foreach (var directory in subDirectories)
            {
                ScanRecursive(path + "/" + Path.GetFileName(directory));
            }
        }
    }
}