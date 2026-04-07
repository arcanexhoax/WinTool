using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WinTool.Services;
using WinTool.ViewModels;
using WinTool.Views.Features;
using WinTool.Views.Settings;
using WinTool.Views.Shortcuts;

namespace WinTool.Views;

public partial class MainWindow : FluentWindow
{
    private readonly Dictionary<string, FrameworkElement> _tabCache = [];
    private readonly MainViewModel _viewModel;
    private readonly ViewFactory _viewFactory;

    private bool _allowClose;

    public MainWindow(MainViewModel mainViewModel, ViewFactory viewFactory)
    {
        _viewModel = mainViewModel;
        _viewModel.ShowWindowRequested += OnShowWindowRequested;
        DataContext = mainViewModel;
        _viewFactory = viewFactory;

        InitializeComponent();
    }

    public void ForceClose()
    {
        _allowClose = true;
        Close();
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

    private void OnShowWindowRequested(object? sender, EventArgs e) => Show();

    private void OnWindowActivated(object? sender, EventArgs e) => Show();

    private void OnWindowClosing(object sender, CancelEventArgs e)
    {
        if (_allowClose)
            return;

        e.Cancel = true;
        Hide();
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.ShowWindowRequested -= OnShowWindowRequested;
        base.OnClosed(e);
    }
}
