using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using App.Plugin.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Windows.Storage;

namespace App.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IPluginDiscoveryService _discoveryService;
    private readonly IDynamicLoaderService _loaderService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoadButtonEnabled))]
    private PluginDiscoveryResult? _selectedPlugin;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PlaceholderVisibility))]
    private UIElement? _pluginContent;

    [ObservableProperty]
    private string _statusMessage = "Select a folder to discover plugins";

    public ObservableCollection<PluginDiscoveryResult> DiscoveredPlugins { get; } = new();

    // Logic for UI state without using converters
    public Visibility PlaceholderVisibility => PluginContent == null ? Visibility.Visible : Visibility.Collapsed;
    public bool IsLoadButtonEnabled => SelectedPlugin != null;

    public HomeViewModel(IPluginDiscoveryService discoveryService, IDynamicLoaderService loaderService)
    {
        _discoveryService = discoveryService;
        _loaderService = loaderService;
    }

    [RelayCommand]
    public async Task DiscoverPluginsAsync(StorageFolder folder)
    {
        StatusMessage = "Scanning...";
        DiscoveredPlugins.Clear();

        var results = await _discoveryService.DiscoverPluginsAsync<IPluginView>(folder);
        foreach (var plugin in results) DiscoveredPlugins.Add(plugin);

        StatusMessage = DiscoveredPlugins.Count > 0 ? $"Found {DiscoveredPlugins.Count} plugins." : "No plugins found.";
    }

    [RelayCommand]
    public async Task LoadSelectedPluginAsync()
    {
        if (SelectedPlugin == null) return;
        try
        {
            StatusMessage = $"Loading {SelectedPlugin.Name}...";

            var config = new DynamicLoadConfiguration(
                DllFile: SelectedPlugin.DllFile,
                PriFile: SelectedPlugin.PriFile,
                WinmdFile: null,
                ClassName: SelectedPlugin.ClassName,
                UseXamlReader: true,
                PriRootFolder: SelectedPlugin.SourceFolder
            );

            PluginContent = await _loaderService.LoadComponentAsync(config);
            StatusMessage = $"Active: {SelectedPlugin.Name} ({SelectedPlugin.Version})";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            PluginContent = null;
        }
    }
}