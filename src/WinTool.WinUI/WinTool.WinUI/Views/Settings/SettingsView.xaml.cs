using Microsoft.UI.Xaml.Controls;
using WinTool.ViewModels.Settings;

namespace WinTool.Views.Settings;

public sealed partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
