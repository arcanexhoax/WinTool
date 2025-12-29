using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using WinTool.ViewModels;
using WinTool.Views.Features;
using WinTool.Views.Settings;
using WinTool.Views.Shortcuts;

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

    private void OnNavViewLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var nav = (NavigationView)sender;

        nav.SelectedItem = ShortcutsItem;
        Navigate(typeof(ShortcutsPage), new EntranceNavigationTransitionInfo());
    }

    private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        Type? pageType = null;

        if (args.IsSettingsSelected)
            pageType = typeof(SettingsPage);
        else if (args.SelectedItemContainer == ShortcutsItem)
            pageType = typeof(ShortcutsPage);
        else if (args.SelectedItemContainer == FeaturesItem)
            pageType = typeof(FeaturesPage);

        Navigate(pageType, args.RecommendedNavigationTransitionInfo);
    }

    private void Navigate(Type? pageType, NavigationTransitionInfo navTransitionInfo)
    {
        if (pageType != null && Root.CurrentSourcePageType != pageType)
            Root.Navigate(pageType, null, navTransitionInfo);
    }
}
