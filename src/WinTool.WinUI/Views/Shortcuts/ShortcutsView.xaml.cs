using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WinTool.WinUI.ViewModels.Shortcuts;

namespace WinTool.WinUI.Views.Shortcuts;

public partial class ShortcutsView : Page
{
    public ShortcutsViewModel ViewModel { get; private set; } = null!;

    public ShortcutsView()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        DataContext = ViewModel = (ShortcutsViewModel)e.Parameter;
        Bindings.Update();
    }
}
