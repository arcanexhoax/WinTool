using Microsoft.UI.Xaml.Controls;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public sealed partial class ShortcutsView : UserControl
{
    public ShortcutsView(ShortcutsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
