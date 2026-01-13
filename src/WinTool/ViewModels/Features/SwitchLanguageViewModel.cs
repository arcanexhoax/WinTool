using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using WinTool.Services;
using WinTool.Utils;

namespace WinTool.ViewModels.Features;

public partial class SwitchLanguageViewModel : ObservableObject
{
    private readonly KeyboardLayoutManager _keyboardLayoutManager;

    [ObservableProperty]
    public partial string? CurrentLanguage { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<LanguageViewModel>? AllLanguages { get; set; }

    public event Action<Point>? ShowPopup;

    public SwitchLanguageViewModel(KeyboardLayoutManager keyboardLayoutManager)
    {
        _keyboardLayoutManager = keyboardLayoutManager;
        _keyboardLayoutManager.LayoutChanged += OnLayoutChanged;
        _keyboardLayoutManager.LayoutsListChanged += OnLayoutsListChanged;

        OnLayoutsListChanged(_keyboardLayoutManager.AllCultures);
    }

    private void OnLayoutsListChanged(IEnumerable<CultureInfo> allLayouts)
    {
        var allLanguages = allLayouts.Select(layout => new LanguageViewModel(layout, GetThreeLettersNativeName(layout)));
        App.Current.Dispatcher.Invoke(() => AllLanguages = [.. allLanguages]);
    }

    private void OnLayoutChanged(CultureInfo newCulture)
    {
        var caretRect = CaretHelper.GetCaretRect();

        if (caretRect == null)
            return;

        CurrentLanguage = GetThreeLettersNativeName(newCulture);

        foreach (var language in AllLanguages!)
        {
            language.IsSelected = language.CultureInfo.Equals(newCulture);
        }

        ShowPopup?.Invoke(new Point(caretRect.Value.right, caretRect.Value.bottom));
    }

    private string GetThreeLettersNativeName(CultureInfo culture)
    {
        return culture.NativeName.Length > 3 ? culture.NativeName[..3].ToUpper() : culture.NativeName.ToUpper();
    }
}
