using CommunityToolkit.WinUI;
using GlobalKeyInterceptor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using WinTool.WinUI.Models;
using WinTool.WinUI.Properties;
using WinTool.WinUI.Views.Shortcuts;

namespace WinTool.WinUI.Services;

public class DialogService(IServiceProvider serviceProvider)
{
    private readonly SemaphoreSlim _dialogLock = new(1, 1);

    public async Task ShowErrorAsync(string message)
    {
        await ShowAsync(async root =>
        {
            var dialog = new ContentDialog
            {
                XamlRoot = root,
                Title = Resources.Error,
                Content = message,
                CloseButtonText = Resources.Close,
                DefaultButton = ContentDialogButton.Close
            };

            await dialog.ShowAsync();
        });
    }

    public async Task<Result<Shortcut>> EditShortcutAsync(EditShortcutInput input)
    {
        Result<Shortcut> result = new(false);

        await ShowAsync(async root =>
        {
            var dialog = serviceProvider.GetRequiredService<EditShortcutView>();
            dialog.XamlRoot = root;
            dialog.RequestedTheme = root.Content is FrameworkElement element
                ? element.ActualTheme
                : ElementTheme.Default;

            try
            {
                dialog.ViewModel.OnShow(input, r => result = r);
                await dialog.ShowAsync();
            }
            finally
            {
                dialog.ViewModel.OnClose();
            }
        });

        return result;
    }

    public async Task<Result<RunWithArgsOutput>> ShowRunWithArgsAsync(string filePath)
    {
        Result<RunWithArgsOutput> result = new(false);

        await App.Dispatcher.EnqueueAsync(async () =>
        {
            await _dialogLock.WaitAsync();
            RunWithArgsWindow? window = null;

            try
            {
                var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

                window = serviceProvider.GetRequiredService<RunWithArgsWindow>();
                window.Closed += (_, _) => closed.TrySetResult();
                window.ViewModel.OnShow(filePath, r =>
                {
                    result = r;
                    window.Close();
                });
                window.Activate();

                await closed.Task;
            }
            finally
            {
                window?.ViewModel.OnClose();
                _dialogLock.Release();
            }
        });

        return result;
    }

    private Task ShowAsync(Func<XamlRoot, Task> show)
    {
        return App.Dispatcher.EnqueueAsync(async () =>
        {
            await _dialogLock.WaitAsync();

            try
            {
                App.MainWindow?.Activate();

                if (App.MainWindow?.Content is FrameworkElement { XamlRoot: { } root })
                    await show(root);
            }
            finally
            {
                _dialogLock.Release();
            }
        });
    }
}
