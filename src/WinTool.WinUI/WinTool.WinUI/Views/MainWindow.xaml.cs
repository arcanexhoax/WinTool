using WinTool.ViewModels;

namespace WinTool.Views;

public sealed partial class MainWindow : WindowBase
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
