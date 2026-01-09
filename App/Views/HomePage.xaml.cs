using System;
using App.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace App.Views;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel
    {
        get;
    }

    public HomePage()
    {
        // Resolved from your DI setup in App.xaml.cs
        ViewModel = App.GetService<HomeViewModel>();
        this.InitializeComponent();
    }

    private async void OnPickFolderClick(object sender, RoutedEventArgs e)
    {
        var folderPicker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            FileTypeFilter = { "*" }
        };

        // Initialize with the HWND of the main window
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(folderPicker, hwnd);

        var folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            await ViewModel.DiscoverPluginsCommand.ExecuteAsync(folder);
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        // Help the GC and WinUI cleanup dynamic resources
        ViewModel.PluginContent = null;
        base.OnNavigatedFrom(e);
    }
}