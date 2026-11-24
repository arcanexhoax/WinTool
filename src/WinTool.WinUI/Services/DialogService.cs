using CommunityToolkit.WinUI;
using GlobalKeyInterceptor;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using WinTool.WinUI.Models;
using WinTool.WinUI.Properties;

namespace WinTool.WinUI.Services;

public class DialogService
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

    public Task<Result<Shortcut>> EditShortcutAsync(EditShortcutInput input)
    {
        return Task.FromResult(new Result<Shortcut>(false));
    }

    public Task<Result<RunWithArgsOutput>> ShowRunWithArgsAsync(string filePath)
    {
        return Task.FromResult(new Result<RunWithArgsOutput>(false));
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
