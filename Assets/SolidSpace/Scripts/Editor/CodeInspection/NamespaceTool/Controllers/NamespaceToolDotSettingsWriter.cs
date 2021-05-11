using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SolidSpace.Editor.CodeInspection.NamespaceTool
{
    public class NamespaceToolDotSettingsWriter
    {
        private const string LocalDirectoryNameRegex = @"\/((?i)assets\/.*)";
        
        public void Write(string toFile, IEnumerable<string> foldersToIgnore)
        {
            using var writer = new StreamWriter(toFile);
            
            writer.Write("<wpf:ResourceDictionary xml:space=\"preserve\" ");
            writer.Write("xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" ");
            writer.Write("xmlns:s=\"clr-namespace:System;assembly=mscorlib\" ");
            writer.Write("xmlns:ss=\"urn:shemas-jetbrains-com:settings-storage-xaml\" ");
            writer.Write("xmlns:wpf=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">");
            writer.WriteLine();

            foreach (var path in foldersToIgnore)
            {
                var localPath = Regex.Match(path, LocalDirectoryNameRegex).Groups[1].Value;
                localPath = localPath.ToLower().Replace("/", "_005C");

                writer.Write("\t<s:Boolean x:Key=\"/Default/CodeInspection/NamespaceProvider/NamespaceFoldersToSkip/=");
                writer.Write(localPath);
                writer.Write("/@EntryIndexedValue\">True</s:Boolean>");
                writer.WriteLine();
            }
        }
    }
}