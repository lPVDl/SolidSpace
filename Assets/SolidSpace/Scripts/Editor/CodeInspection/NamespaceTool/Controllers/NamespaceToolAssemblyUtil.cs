using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SolidSpace.Editor.CodeInspection.NamespaceTool
{
    public class NamespaceToolAssemblyUtil
    {
        public void Scan(NamespaceToolConfig config, ICollection<NamespaceToolEntityInfo> output)
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
                    
                    output.Add(new NamespaceToolEntityInfo
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