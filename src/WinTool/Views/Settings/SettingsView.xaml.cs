using System.Windows.Controls;
using System.Windows.Threading;
using WinTool.ViewModels.Settings;

namespace WinTool.Views.Settings;

public partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }

    public void OpenAbout()
    {
        AboutExpander.IsExpanded = true;
        Dispatcher.BeginInvoke(() => SettingsScrollViewer.ScrollToEnd(), DispatcherPriority.Loaded);
    }
}
