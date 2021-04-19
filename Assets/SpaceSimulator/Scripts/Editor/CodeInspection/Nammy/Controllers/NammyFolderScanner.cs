using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SpaceSimulator.Editor.CodeFlow
{
    public class NammyFolderScanner
    {
        public void Scan(NammyConfig config, ICollection<NammyFolderInfo> output)
        {
            var projectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
            var rootPath = Path.Combine(projectRoot, config.ScriptsRoot);
            
            var state = new NammyFolderInfo
            {
                fullPath = rootPath,
                regexId = -1
            };
            
            output.Clear();
            
            ScanRecursive(state, config.FolderFilters, output);
        }

        private void ScanRecursive(NammyFolderInfo state, IReadOnlyList<NammyFolderFilter> filters, ICollection<NammyFolderInfo> output)
        {
            for (var i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                if (!filter.enabled)
                {
                    continue;
                }
                
                if (!Regex.IsMatch(state.fullPath, filter.regex))
                {
                    continue;
                }

                state.regexId = i;
                output.Add(state);
                break;
            }

            var subDirectories = Directory.GetDirectories(state.fullPath);
            foreach (var directory in subDirectories)
            {
                state.fullPath = directory;

                ScanRecursive(state, filters, output);
            }
        }
    }
}