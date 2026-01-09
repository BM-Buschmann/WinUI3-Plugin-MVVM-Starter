using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace App.Plugin.Contracts;
public record DynamicLoadConfiguration(
    StorageFile DllFile,
    StorageFile PriFile,
    StorageFile? WinmdFile,
    string ClassName,
    bool UseXamlReader,
    StorageFolder? PriRootFolder = null
);
