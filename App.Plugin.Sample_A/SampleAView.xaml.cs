using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.ComponentModel.Composition;
using App.Plugin.Contracts;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App.Plugin.Sample_A;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
/// 
// MEF finds this class because of the Export attribute
[Export(typeof(IPluginView))]
public sealed partial class SampleAView : Page, IPluginView
{
    public SampleAView()
    {
        InitializeComponent();
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        ((Button)sender).Content = "WinUI's managed world says hi!";
    }
}
