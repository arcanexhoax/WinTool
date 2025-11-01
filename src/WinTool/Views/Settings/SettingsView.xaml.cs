using System.Windows.Controls;
using WinTool.ViewModels.Settings;

namespace WinTool.Views.Settings;

public partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }
}
