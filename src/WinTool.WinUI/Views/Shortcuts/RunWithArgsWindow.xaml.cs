using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using Windows.Graphics;
using WinRT.Interop;
using WinTool.WinUI.Native;
using WinTool.WinUI.ViewModels.Shortcuts;

namespace WinTool.WinUI.Views.Shortcuts;

public partial class RunWithArgsWindow : Window
{
    public RunWithArgsViewModel ViewModel { get; }

    public RunWithArgsWindow(RunWithArgsViewModel runWithArgsVm)
    {
        ViewModel = runWithArgsVm;

        InitializeComponent();

        // TODO use ex
        ExtendsContentIntoTitleBar = true;

        if (App.MainWindow?.Content is FrameworkElement root)
            ((FrameworkElement)Content).RequestedTheme = root.ActualTheme;

        AppWindow.Resize(new SizeInt32(420, 440));
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Resources", "icon.ico"));

        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.SetBorderAndTitleBar(true, false);
            presenter.PreferredMinimumWidth = 420;
            presenter.PreferredMinimumHeight = 440;
        }

        var workArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Primary).WorkArea;
        AppWindow.Move(new PointInt32(
            workArea.X + ((workArea.Width - AppWindow.Size.Width) / 2), 
            workArea.Y + ((workArea.Height - AppWindow.Size.Height) / 2)));
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (!e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed)
            return;

        for (var element = e.OriginalSource as DependencyObject; element is not null; element = VisualTreeHelper.GetParent(element))
        {
            if (element is TextBox or ButtonBase)
                return;
        }

        e.Handled = true;
        NativeMethods.DragMove(WindowNative.GetWindowHandle(this));
    }

    private void OnArgsLoaded(object sender, RoutedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            INPUT[] inputs = [new() { Type = INPUTTYPE.INPUTMOUSE }];
            NativeMethods.SendInput(1, inputs, INPUT.Size);
            NativeMethods.SetForegroundWindow(WindowNative.GetWindowHandle(this));

            ArgsTextBox.Focus(FocusState.Programmatic);

            if (ViewModel.IsTextSelected)
                ArgsTextBox.SelectAll();
        });
    }

    private void OnRunInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.RunCommand.Execute(null);
        args.Handled = true;
    }

    private void OnCancelInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        ViewModel.CloseWindowCommand.Execute(null);
        args.Handled = true;
    }
}
