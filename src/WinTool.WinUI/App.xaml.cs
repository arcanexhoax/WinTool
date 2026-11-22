using Microsoft.UI.Xaml;
using System.Globalization;

namespace WinTool.WinUI;

public partial class App : Application
{
    private Window? _mainWindow;

    public static CultureInfo SystemUICulture { get; } = CultureInfo.CurrentUICulture;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }
}
