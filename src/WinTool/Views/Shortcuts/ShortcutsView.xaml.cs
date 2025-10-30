using System.Windows.Controls;
using WinTool.ViewModels.Shortcuts;

namespace WinTool.Views.Shortcuts;

public partial class ShortcutsView : UserControl
{
    public ShortcutsView(ShortcutsViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }
}
