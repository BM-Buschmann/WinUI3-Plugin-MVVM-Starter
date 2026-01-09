using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using App.Plugin.Contracts;
using Windows.Storage;

namespace App.Plugin.Services;

public class PluginDiscoveryService : IPluginDiscoveryService
{
    public async Task<List<PluginDiscoveryResult>> DiscoverPluginsAsync<T>(StorageFolder folder)
    {
        Debug.WriteLine($"[Discovery] === Starting MLC Scan ===");
        Debug.WriteLine($"[Discovery] Target Interface: {typeof(T).Name}");
        Debug.WriteLine($"[Discovery] Source Folder: {folder.Path}");

        var results = new List<PluginDiscoveryResult>();

        IReadOnlyList<StorageFile> files;
        try
        {
            files = await folder.GetFilesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Discovery] ! ERROR: Could not access folder files: {ex.Message}");
            return results;
        }

        var dllFiles = files.Where(f => f.FileType == ".dll").ToList();
        Debug.WriteLine($"[Discovery] Found {dllFiles.Count} DLLs to inspect.");

        var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
        var paths = Directory.GetFiles(runtimeDirectory, "*.dll").ToList();
        paths.Add(typeof(T).Assembly.Location);
        paths.AddRange(dllFiles.Select(f => f.Path));

        var resolver = new PathAssemblyResolver(paths);
        using var mlc = new MetadataLoadContext(resolver);

        foreach (var dllFile in dllFiles)
        {
            try
            {
                Debug.WriteLine($"[Discovery] --> Inspecting DLL: {dllFile.Name}");

                var assembly = mlc.LoadFromAssemblyPath(dllFile.Path);
                var assemblyName = assembly.GetName();
                Debug.WriteLine($"[Discovery]     Identity: {assemblyName.Name}, Version: {assemblyName.Version}");

                var pluginType = assembly.GetTypes().FirstOrDefault(t =>
                    t.GetInterfaces().Any(i => i.Name == typeof(T).Name));

                if (pluginType != null)
                {
                    Debug.WriteLine($"[Discovery]     MATCH: Found type '{pluginType.FullName}' implementing {typeof(T).Name}");

                    var baseName = Path.GetFileNameWithoutExtension(dllFile.Name);
                    var priFile = files.FirstOrDefault(f => f.Name.Equals($"{baseName}.pri", StringComparison.OrdinalIgnoreCase));

                    if (priFile != null)
                    {
                        Debug.WriteLine($"[Discovery]     MATCH: Found associated PRI: {priFile.Name}");
                        results.Add(new PluginDiscoveryResult(
                            Name: assemblyName.Name ?? baseName,
                            Version: assemblyName.Version?.ToString() ?? "1.0.0.0",
                            ClassName: pluginType.FullName,
                            DllFile: dllFile,
                            PriFile: priFile,
                            SourceFolder: folder
                        ));
                    }
                    else
                    {
                        Debug.WriteLine($"[Discovery]     ! SKIP: No matching .pri file found for {dllFile.Name}");
                    }
                }
                else
                {
                    Debug.WriteLine($"[Discovery]     No implementation of {typeof(T).Name} found in this assembly.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Discovery]     ! ERROR scanning {dllFile.Name}: {ex.GetType().Name} - {ex.Message}");
            }
        }

        Debug.WriteLine($"[Discovery] === Scan Complete. Results: {results.Count} plugins identified ===");
        return results;
    }
}