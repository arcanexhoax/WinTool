#pragma warning disable WPF0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinTool.Options;
using WinTool.Services;
using WinTool.ViewModels;
using WinTool.ViewModels.Settings;
using WinTool.Views.Features;
using WinTool.Views.Settings;
using WinTool.Views.Shortcuts;

namespace WinTool.Views;

public partial class MainWindow : FluentWindow
{
    private readonly Dictionary<string, FrameworkElement> _tabCache = [];
    private readonly ViewFactory _viewFactory;
    private readonly IOptionsMonitor<SettingsOptions> _settingsOptions;

    public MainWindow(MainViewModel mainViewModel, ViewFactory viewFactory, IOptionsMonitor<SettingsOptions> settingsOptions)
    {
        mainViewModel.ShowWindowRequested += (_, _) => Show();

        DataContext = mainViewModel;
        _viewFactory = viewFactory;
        _settingsOptions = settingsOptions;
        _settingsOptions.OnChange((o, _) =>
        {
            if (SetThemeMode(o.AppTheme))
                SetCustomThemeColors(o.AppTheme);
        });

        SetThemeMode(_settingsOptions.CurrentValue.AppTheme);
        SetCustomThemeColors(_settingsOptions.CurrentValue.AppTheme);

        InitializeComponent();
    }

    private bool SetThemeMode(AppTheme selectedTheme)
    {
        var selectedThemeName = selectedTheme.ToString();

        if (App.Current.ThemeMode.Value != selectedThemeName)
        {
            App.Current.ThemeMode = new ThemeMode(selectedThemeName);
            return true;
        }

        return false;
    }

    private void SetCustomThemeColors(AppTheme selectedTheme)
    {
        var actualTheme = selectedTheme == AppTheme.System ? GetSystemTheme() : selectedTheme;

        if (actualTheme == AppTheme.Dark)
        {
            Application.Current.Resources["EmptyBackdropBrush"] = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20));
            Application.Current.Resources["AcrylicLayoutBrush"] = new SolidColorBrush(Color.FromArgb(0x99, 0x00, 0x00, 0x00));
        }
        else
        {
            Application.Current.Resources["EmptyBackdropBrush"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            Application.Current.Resources["AcrylicLayoutBrush"] = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        Tabs.SelectedIndex = 0;
    }

    private void OnTabSelected(object sender, SelectionChangedEventArgs e)
    {
        if (Tabs.SelectedItem is not ListViewItem item)
            return;

        if (!_tabCache.TryGetValue(item.Name, out var view))
        {
            view = item.Name switch
            {
                "ShortcutsTab" => _viewFactory.Create<ShortcutsView>(),
                "FeaturesTab" => _viewFactory.Create<FeaturesView>(),
                "SettingsTab" => _viewFactory.Create<SettingsView>(),
                _ => null
            };

            if (view is null)
                return;

            _tabCache[item.Name] = view;
        }

        TabContent.Content = view;
    }

    private void OnWindowActivated(object? sender, EventArgs e) => Show();

    private void OnWindowClosing(object sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private AppTheme GetSystemTheme()
    {
        const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        using var key = Registry.CurrentUser.OpenSubKey(keyPath);

        if (key == null)
            return AppTheme.Light;

        var value = key.GetValue("AppsUseLightTheme");
        return value is int i && i == 0 ? AppTheme.Dark : AppTheme.Light;
    }
}
