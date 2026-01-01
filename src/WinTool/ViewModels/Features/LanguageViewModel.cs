using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace WinTool.ViewModels.Features;

public partial class LanguageViewModel(CultureInfo cultureInfo, string name) : ObservableObject
{
    public CultureInfo CultureInfo { get; } = cultureInfo;

    [ObservableProperty]
    public partial string Name { get; set; } = name;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}
