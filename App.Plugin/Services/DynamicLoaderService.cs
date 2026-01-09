using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using App.Plugin.Contracts;
using DynamicXaml.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using MrmLib;
using Windows.Storage;

namespace App.Plugin.Services;

public class DynamicLoaderService : IDynamicLoaderService
{
    private readonly StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

    // Tracks plugins already registered in this session: Key = AssemblyName_Version
    private static readonly ConcurrentDictionary<string, string> _sessionCache = new();

    public async Task<UIElement> LoadComponentAsync(DynamicLoadConfiguration config)
    {
        var sw = Stopwatch.StartNew();
        var assemblyName = AssemblyName.GetAssemblyName(config.DllFile.Path);
        string version = assemblyName.Version?.ToString() ?? "1.0.0.0";
        string identityKey = $"{assemblyName.Name}_{version}";

        string targetDllName = $"{identityKey}.dll";
        string targetPriName = $"{identityKey}.pri";

        // 1. Smart Copy Check
        bool exists = File.Exists(Path.Combine(_localFolder.Path, targetDllName));
        StorageFile dll, pri;

        if (!exists)
        {
            Debug.WriteLine("[Loader] First time: Copying and Repairing PRI...");
            dll = await config.DllFile.CopyAsync(_localFolder, targetDllName);

            // --- PRI REPAIR LOGIC (MrmLib) ---
            var priData = await PriFile.LoadAsync(config.PriFile);
            // This embeds the XAML and fixes internal paths using the source folder
            await priData.ReplacePathCandidatesWithEmbeddedDataAsync(config.PriRootFolder);

            pri = await _localFolder.CreateFileAsync(targetPriName, CreationCollisionOption.ReplaceExisting);
            await priData.WriteAsync(pri);
            // --------------------------------
        }
        else
        {
            Debug.WriteLine("[Loader] Cache Hit: Using existing files.");
            dll = await _localFolder.GetFileAsync(targetDllName);
            pri = await _localFolder.GetFileAsync(targetPriName);
        }

        // 2. Session Registration (Once per App Lifetime)
        if (!_sessionCache.ContainsKey(identityKey))
        {
            DynamicLoader.LoadPri(pri);
            var asm = Assembly.LoadFrom(dll.Path);

            var providerTypeNames = await XamlMetadataProviderHelper.GetProviderTypeNamesFromAssemblyAsync(dll);
            foreach (var pName in providerTypeNames ?? Enumerable.Empty<string>())
            {
                var pType = asm.GetType(pName);
                if (pType != null)
                {
                    var provider = (IXamlMetadataProvider)Activator.CreateInstance(pType);
                    DynamicLoader.RegisterXamlMetadataProvider(provider);
                }
            }
            _sessionCache.TryAdd(identityKey, dll.Path);
        }

        // 3. Load UI
        var shortName = config.ClassName.Split('.').Last();
        var ns = config.ClassName[..^(shortName.Length + 1)];
        var xaml = $"<Page xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:lib='using:{ns}'><lib:{shortName}/></Page>";

        try
        {
            var element = (UIElement)XamlReader.Load(xaml);
            Debug.WriteLine($"[Loader] Load Successful ({sw.ElapsedMilliseconds}ms)");
            return element;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Loader] XamlReader Error: {ex.Message}");
            throw;
        }
    }
}