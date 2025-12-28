using Microsoft.UI.Windowing;
using WinTool.ViewModels;

namespace WinTool.Views;

public sealed partial class MainWindow : WindowBase
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(titleBar);

        AppWindow.Resize(new Windows.Graphics.SizeInt32(1000, 600));
        AppWindow.SetIcon("Resources/icon.ico");
        AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;

        var presenter = OverlappedPresenter.Create();
        presenter.PreferredMinimumWidth = 900;
        presenter.PreferredMinimumHeight = 400;

        AppWindow.SetPresenter(presenter);
    }
}
