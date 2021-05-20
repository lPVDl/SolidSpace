using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SolidSpace.Editor.Automation.ProjectStructureTool
{
    internal class FileScanner
    {
        private readonly List<string> _blacklistFilters; 
        
        private List<EntityInfo> _outEntities;

        public FileScanner()
        {
            _blacklistFilters = new List<string>();
        }

        public List<EntityInfo> Scan(Config config)
        {
            _outEntities = new List<EntityInfo>();

            _blacklistFilters.Clear();
            _blacklistFilters.AddRange(config.BlackListFilters.Where(f => f.enabled).Select(f => f.regex));

            var appRoot = Application.dataPath;
            var projectRoot = appRoot.Substring(0, appRoot.Length - 7);
            var scanRoot = Path.Combine(projectRoot, config.ScanRoot);
            ScanRecursive(scanRoot, 0);
            
            return _outEntities;
        }

        private void ScanRecursive(string path, int deep)
        {
            if (_blacklistFilters.Any(filter => Regex.IsMatch(path, filter)))
            {
                return;
            }
            
            _outEntities.Add(new EntityInfo
            {
                name = Path.GetFileName(path),
                deep = deep,
                sizeBytes = 0
            });

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                ScanRecursive(directory, deep + 1);
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (_blacklistFilters.Any(filter => Regex.IsMatch(file, filter)))
                {
                    continue;
                }

                var fileInfo = new System.IO.FileInfo(file);
                
                _outEntities.Add(new EntityInfo
                {
                    name = Path.GetFileName(file),
                    deep = deep + 1,
                    sizeBytes = fileInfo.Length
                });
            }
        }
    }
}