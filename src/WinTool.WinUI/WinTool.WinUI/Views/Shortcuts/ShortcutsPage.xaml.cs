using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WinTool.ViewModels.Shortcuts;
using WinTool.WinUI;

namespace WinTool.Views.Shortcuts;

public sealed partial class ShortcutsPage : Page
{
    public ShortcutsPage()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ShortcutsViewModel>();
    }
}
