using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using Windows.Graphics;
using WinTool.WinUI.ViewModels;

namespace WinTool.WinUI.Views;

public partial class MainWindow : Window
{
    private bool _allowClose;

    public MainViewModel ViewModel { get; }

    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        ((FrameworkElement)Content).DataContext = ViewModel = mainViewModel;

        AppWindow.Resize(new SizeInt32(1000, 600));
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Resources", "icon.ico"));

        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = 900;
            presenter.PreferredMinimumHeight = 400;
        }

        Tabs.SelectedItem = ShortcutsTab;

        AppWindow.Closing += OnClosing;
        Closed += OnClosed;
    }

    public void ForceClose()
    {
        _allowClose = true;
        Close();
    }

    public void OpenAboutSettings()
    {
        Tabs.SelectedItem = SettingsTab;

        //if (TabContent.Content is SettingsView settingsView)
        //    settingsView.OpenAbout();
    }

    public void SetUpdateOverlay(bool isUpdateAvailable)
    {
        //TaskbarInfo.Overlay = isUpdateAvailable
        //    ? new BitmapImage(new Uri("pack://application:,,,/Resources/update-overlay.png"))
        //    : null;
    }

    private void OnClosing(AppWindow sender, AppWindowClosingEventArgs e)
    {
        if (_allowClose)
            return;

        e.Cancel = true;

        AppWindow.Hide();
        ViewModel.OnWindowHidden();
    }

    private void OnClosed(object sender, WindowEventArgs e)
    {
        //foreach (var view in _tabCache.Values)
        //{
        //    if (view.DataContext is IDisposable disposable)
        //        disposable.Dispose();
        //}
    }
}
