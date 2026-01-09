using Windows.Storage;

namespace App.Plugin.Contracts;

public record PluginDiscoveryResult(
    string Name,        // From AssemblyProduct or AssemblyName
    string Version,     // From AssemblyVersion
    string ClassName,   // Automatically detected Full Name
    StorageFile DllFile,
    StorageFile PriFile,
    StorageFolder SourceFolder
);