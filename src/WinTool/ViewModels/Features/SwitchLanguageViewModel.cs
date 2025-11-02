using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using WinTool.Options;
using WinTool.Services;
using WinTool.Utils;

namespace WinTool.ViewModels.Features;

public class SwitchLanguageViewModel : ObservableObject
{
    private readonly KeyboardLayoutManager _keyboardLayoutManager;
    private readonly IOptionsMonitor<FeaturesOptions> _featuresOptions;

    public string? CurrentLanguage
    {
        get; set => SetProperty(ref field, value);
    }

    public ObservableCollection<LanguageViewModel>? AllLanguages
    {
        get; set => SetProperty(ref field, value);
    }

    public event Action<Point>? ShowPopup;

    public SwitchLanguageViewModel(KeyboardLayoutManager keyboardLayoutManager, IOptionsMonitor<FeaturesOptions> featuresOptions)
    {
        _keyboardLayoutManager = keyboardLayoutManager;
        _keyboardLayoutManager.LayoutChanged += OnLayoutChanged;
        _keyboardLayoutManager.LayoutsListChanged += OnLayoutsListChanged;
        _featuresOptions = featuresOptions;
        _featuresOptions.OnChange((o, _) => 
        {
            if (o.EnableSwitchLanguagePopup)
                Start();
            else
                Stop();
        });

        OnLayoutsListChanged(_keyboardLayoutManager.AllCultures);

        if (_featuresOptions.CurrentValue.EnableSwitchLanguagePopup)
            Start();
    }

    public void Start() => _keyboardLayoutManager.Start();

    public void Stop() => _keyboardLayoutManager.Stop();

    private void OnLayoutsListChanged(IEnumerable<CultureInfo> allLayouts)
    {
        var allLanguages = allLayouts.Select(layout => new LanguageViewModel(layout, GetThreeLettersNativeName(layout)));
        AllLanguages = [.. allLanguages];
    }

    private void OnLayoutChanged(CultureInfo newCulture)
    {
        var caretRect = CarretHelper.GetCaretRect();

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
