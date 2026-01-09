using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace App.Plugin.Contracts;
public interface IPluginDiscoveryService
{
    /// <summary>
    /// Scans a folder for DLLs that export a specific interface T.
    /// </summary>
    Task<List<PluginDiscoveryResult>> DiscoverPluginsAsync<T>(StorageFolder folder);
}