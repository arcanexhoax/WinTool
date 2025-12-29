using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WinTool.ViewModels.Settings;
using WinTool.WinUI;

namespace WinTool.Views.Settings;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<SettingsViewModel>();
    }
}
