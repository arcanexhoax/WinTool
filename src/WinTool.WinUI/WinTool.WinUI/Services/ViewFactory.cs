using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace WinTool.Services;

public class ViewFactory(IServiceProvider sp)
{
    private readonly IServiceProvider _sp = sp;

    public T CreateView<T>() where T : UserControl => _sp.GetRequiredService<T>();

    public T CreateWindow<T>() where T : Window => _sp.GetRequiredService<T>();
}

