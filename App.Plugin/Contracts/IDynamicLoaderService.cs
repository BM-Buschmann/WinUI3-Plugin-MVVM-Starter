using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace App.Plugin.Contracts;
public interface IDynamicLoaderService
{
    /// <summary>
    /// Orchestrates the loading of a remote UI component.
    /// </summary>
    Task<UIElement> LoadComponentAsync(DynamicLoadConfiguration config);
}