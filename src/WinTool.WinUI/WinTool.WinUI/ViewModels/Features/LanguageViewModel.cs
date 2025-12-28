using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;

namespace WinTool.ViewModels.Features;

public partial class LanguageViewModel : ObservableObject
{
    public CultureInfo CultureInfo { get; }

    public string Name
    {
        get; set => SetProperty(ref field, value);
    }

    public bool IsSelected
    {
        get; set => SetProperty(ref field, value);
    }

    public LanguageViewModel(CultureInfo cultureInfo, string name)
    {
        CultureInfo = cultureInfo;
        Name = name;
    }
}
