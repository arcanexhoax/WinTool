using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;

namespace WinTool.Services;

public class ViewFactory(IServiceProvider sp)
{
    private readonly IServiceProvider _sp = sp;

    public T Create<T>() where T : ContentControl => _sp.GetRequiredService<T>();
}

