using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace WinTool.Services;

public class WindowFactory(IServiceProvider sp)
{
    private readonly IServiceProvider _sp = sp;

    public T Create<T>() where T : Window => _sp.GetRequiredService<T>();
}

