using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;
using WinTool.Models;
using WinTool.Views;

namespace WinTool.Services;

public class ViewFactory(IServiceProvider sp)
{
    private readonly IServiceProvider _sp = sp;

    public T Create<T>() where T : ContentControl => _sp.GetRequiredService<T>();

    public Result<TOut> ShowDialog<TDialog, TIn, TOut>(TIn input) where TDialog : DialogWindow<TIn, TOut>
    {
        Result<TOut>? result = null;

        App.Current.Dispatcher.Invoke(() =>
        {
            var dialog = Create<TDialog>();
            result = dialog.ShowDialog(input);
        });

        return result ?? new Result<TOut>(false);
    }
}

