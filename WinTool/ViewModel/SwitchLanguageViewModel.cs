using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using UIAutomationClient;
using WinTool.Native;
using WinTool.Options;
using WinTool.Services;

namespace WinTool.ViewModel;

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
        var caretRect = GetCaretRect();

        if (caretRect == null)
            return;

        CurrentLanguage = GetThreeLettersNativeName(newCulture);

        foreach (var language in AllLanguages!)
        {
            language.IsSelected = language.CultureInfo.Equals(newCulture);
        }

        ShowPopup?.Invoke(new Point(caretRect.Value.right, caretRect.Value.bottom));
    }

    private RECT? GetCaretRect()
    {
        var info = NativeMethods.GetGuiThreadInfo();

        if (info == null)
            return null;

        var hwndFocus = info.Value.hwndFocus;
        var caretRect = GetAccessibleCaretRect(hwndFocus);

        if (RectValid(caretRect))
            return caretRect;

        caretRect = GetWinApiCaretRect(hwndFocus);

        if (RectValid(caretRect))
            return caretRect;

        return null;
    }

    private RECT? GetAccessibleCaretRect(nint hwnd)
    {
        var guid = typeof(IAccessible).GUID;
        object? accessibleObject = null;
        var retVal = NativeMethods.AccessibleObjectFromWindow(hwnd, NativeMethods.OBJID_CARET, ref guid, ref accessibleObject);

        if (retVal != 0 || accessibleObject is not IAccessible accessible)
            return null;

        accessible.accLocation(out int left, out int top, out int width, out int height, NativeMethods.CHILDID_SELF);

        return new RECT() 
        { 
            bottom = top + height, 
            left = left, 
            right = left + width, 
            top = top 
        };
    }

    private RECT GetWinApiCaretRect(nint hwnd)
    {
        // Try WinAPI
        uint idAttach = 0;
        uint curThreadId = 0;
        POINT caretPoint;

        try
        {
            idAttach = NativeMethods.GetWindowThreadProcessId(hwnd, out uint id);
            curThreadId = NativeMethods.GetCurrentThreadId();

            // To attach to current thread
            var sa = NativeMethods.AttachThreadInput(idAttach, curThreadId, true);
            var caretPos = NativeMethods.GetCaretPos(out caretPoint);
            NativeMethods.ClientToScreen(hwnd, ref caretPoint);
        }
        finally
        {
            // To dettach from current thread
            var sd = NativeMethods.AttachThreadInput(idAttach, curThreadId, false);
        }

        return new RECT()
        {
            left = caretPoint.X,
            top = caretPoint.Y,
            bottom = caretPoint.Y + 20,
            right = caretPoint.X + 1
        };
    }

    private bool RectValid(RECT? rect)
    {
        return rect is { bottom: > 0, left: > 0, right: > 0, top: > 0 };
    }

    private string GetThreeLettersNativeName(CultureInfo culture)
    {
        return culture.NativeName.Length > 3 ? culture.NativeName[..3].ToUpper() : culture.NativeName.ToUpper();
    }
}
