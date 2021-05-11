using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SolidSpace.Automation.NamespaceTool
{
    internal class AssemblyUtil
    {
        public void Scan(Config config, ICollection<EntityInfo> output)
        {
            output.Clear();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var filters = config.AssemblyFilters;
            
            for (var i = 0; i < assemblies.Length; i++)
            {
                var assemblyName = assemblies[i].GetName().Name;
                
                for (var j = 0; j < filters.Count; j++)
                {
                    var filter = filters[j];

                    if (!filter.enabled)
                    {
                        continue;
                    }

                    if (!Regex.IsMatch(assemblyName, filter.regex))
                    {
                        continue;
                    }
                    
                    output.Add(new EntityInfo
                    {
                        name = assemblyName,
                        regexId = j
                    });
                    
                    break;
                }
            }
        }

        public string AssemblyToFileName(string assemblyName)
        {
            return assemblyName + ".csproj.DotSettings";
        }
    }
}