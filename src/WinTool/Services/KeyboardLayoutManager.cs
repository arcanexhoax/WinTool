using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WinTool.Native;
using WinTool.Options;

namespace WinTool.Services;

public class KeyboardLayoutManager : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IKeyInterceptor _keyInterceptor;
    private readonly IOptionsMonitor<FeaturesOptions> _featuresOptions;

    private int _previewLayoutIndex;
    private nint _lastLayout;
    private nint[]? _allLayouts;
    private CancellationTokenSource? _checkLayoutCts;
    private Shortcut? _waitingShortcut;
    private bool _isStarted;
    private bool _waitingForWinRelease;

    public IEnumerable<CultureInfo> AllCultures
    {
        get; private set
        {
            field = value;
            _logger.LogInformation("Available keyboard layouts: {Layouts}", string.Join(", ", value.Select(c => c.NativeName)));
            LayoutsListChanged?.Invoke(value);
        }
    } = [];

    public event Action<CultureInfo>? PreviewLayoutChanged;
    public event Action<CultureInfo>? LayoutChanged;
    public event Action<IEnumerable<CultureInfo>>? LayoutsListChanged;

    public KeyboardLayoutManager(ILogger<KeyboardLayoutManager> logger, IKeyInterceptor keyInterceptor, IOptionsMonitor<FeaturesOptions> featuresOptions)
    {
        _logger = logger;
        _keyInterceptor = keyInterceptor;
        _featuresOptions = featuresOptions;
        _featuresOptions.OnChange((o, _) =>
        {
            if (o.EnableInputPopup)
                Start();
            else
                Stop();
        });
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_featuresOptions.CurrentValue.EnableInputPopup)
            Start();

        return Task.CompletedTask;
    }

    public void Start()
    {
        if (_isStarted)
            return;

        _isStarted = true;
        _lastLayout = GetCurrentKeyboardLayout();
        _allLayouts = GetKeyboardLayoutsUsingWinApi();
        AllCultures = OrderKeyboardLayouts(_allLayouts);

        _keyInterceptor.ShortcutPressed += OnShortcutPressed;
        _logger.LogInformation("KeyboardLayoutManager started");
    }

    public void Stop()
    {
        if (!_isStarted)
            return;

        _isStarted = false;
        _checkLayoutCts?.Cancel();
        _keyInterceptor.ShortcutPressed -= OnShortcutPressed;
        _logger.LogInformation("KeyboardLayoutManager stopped");
    }

    private async void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
    {
        // User can switch keyboard layout with Ctrl + Shift/Shift + Ctrl/Alt + Shift/Shift + Alt
        // But if the shortcut is Shift + Alt and it is in Up state, Windows will send Shift + Ctrl (Up)
        // So we need to track the correct shortcut and check the current layout only after that
        if (IsLayoutSwitchModifiers(e.Shortcut) && e.Shortcut.State is KeyState.Down)
        {
            _waitingShortcut = e.Shortcut;
        }
        // It could be Shift (Down), Shift + Alt (Down), Alt + Shift (Up), Alt (Up) sequence that changes the layout
        else if (_waitingShortcut is not null
            && e.Shortcut.State is KeyState.Up
            && (e.Shortcut.Key == _waitingShortcut.Key && e.Shortcut.Modifier == _waitingShortcut.Modifier
                || AreKeyAndModifierEqual(e.Shortcut.Key, _waitingShortcut.Modifier) && AreKeyAndModifierEqual(_waitingShortcut.Key, e.Shortcut.Modifier)))
        {
            _waitingShortcut = null;
            await CheckLayoutAsync();
        }
        // The second way to change the layout is Win + Space
        // When the user clicking Space while Win is pressed, the current layout does NOT change
        // The layout will only change after releasing Win (not in all cases, but it's ok)
        // NOTE: Windows swallows Space key down events after the first one while Win is held,
        // but Space key up events still come through, so we use those to cycle the preview
        else if (e.Shortcut is { Key: Key.Space, Modifier: KeyModifier.Win, State: KeyState.Down })
        {
            InitWinSpaceTracking();
        }
        else if (e.Shortcut.Key == Key.Space && e.Shortcut.State == KeyState.Up && _waitingForWinRelease)
        {
            AdvancePreviewLayout();
        }
        else if (e.Shortcut.Key.IsWin && e.Shortcut.State == KeyState.Up && _waitingForWinRelease)
        {
            _waitingForWinRelease = false;
            await CheckLayoutAsync();
        }
    }

    private void InitWinSpaceTracking()
    {
        if (_waitingForWinRelease)
            return;

        _waitingForWinRelease = true;
        var cultures = AllCultures.ToArray();
        var currentCulture = ConvertToCultureInfo(_lastLayout);
        _previewLayoutIndex = Array.FindIndex(cultures, c => c.Name == currentCulture.Name);
    }

    private void AdvancePreviewLayout()
    {
        var cultures = AllCultures.ToArray();

        if (cultures.Length > 0)
        {
            _previewLayoutIndex = (_previewLayoutIndex + 1) % cultures.Length;
            PreviewLayoutChanged?.Invoke(cultures[_previewLayoutIndex]);
        }
    }

    private static bool IsLayoutSwitchModifiers(Shortcut shortcut)
    {
        return shortcut.Key.IsAlt && shortcut.Modifier is KeyModifier.Shift
            || shortcut.Key.IsShift && shortcut.Modifier is KeyModifier.Alt
            || shortcut.Key.IsCtrl && shortcut.Modifier is KeyModifier.Shift
            || shortcut.Key.IsShift && shortcut.Modifier is KeyModifier.Ctrl;
    }

    private async Task CheckLayoutAsync()
    {
        _checkLayoutCts?.Cancel();
        _checkLayoutCts = new CancellationTokenSource();
        var token = _checkLayoutCts.Token;

        try
        {
            for (int i = 0; i < 20; i++)
            {
                await Task.Delay(20, token);

                var currentLayout = GetCurrentKeyboardLayout();

                if (currentLayout != _lastLayout)
                {
                    _lastLayout = currentLayout;
                    CheckLayoutsList();

                    var currentCulture = ConvertToCultureInfo(currentLayout);
                    _logger.LogDebug("New layout: {Layout}", currentCulture.NativeName);
                    LayoutChanged?.Invoke(currentCulture);

                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in keyboard layout checking");
        }
    }

    private void CheckLayoutsList()
    {
        var allLayouts = GetKeyboardLayoutsUsingWinApi();

        if (!_allLayouts.SequenceEqual(allLayouts))
        {
            _allLayouts = allLayouts;
            AllCultures = OrderKeyboardLayouts(_allLayouts);
        }
    }

    private nint GetCurrentKeyboardLayout()
    {
        var hWnd = NativeMethods.GetForegroundWindow();

        if (NativeMethods.GetClassName(hWnd) == "ConsoleWindowClass")
        {
            return GetConsoleKeyboardLayout(hWnd);
        }

        var threadInfo = NativeMethods.GetGuiThreadInfo();

        if (threadInfo is null)
            return nint.Zero;

        var activeThreadId = NativeMethods.GetWindowThreadProcessId(threadInfo.Value.hwndFocus, out _);
        var currentLayout = NativeMethods.GetKeyboardLayout(activeThreadId);

        return currentLayout;
    }

    private nint GetConsoleKeyboardLayout(nint consoleHwnd)
    {
        var data = new EnumWindowsData
        {
            ConsoleWindow = consoleHwnd,
            FoundIMEWindow = consoleHwnd
        };

        var dataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(data));

        try
        {
            Marshal.StructureToPtr(data, dataPtr, false);
            NativeMethods.EnumWindows(CallBackEnumWnd, dataPtr);

            data = Marshal.PtrToStructure<EnumWindowsData>(dataPtr);

            if (data.FoundIMEWindow == consoleHwnd || !NativeMethods.IsWindow(data.FoundIMEWindow))
                return nint.Zero;
            
            var threadId = NativeMethods.GetWindowThreadProcessId(data.FoundIMEWindow, out _);
            return NativeMethods.GetKeyboardLayout(threadId);
        }
        finally
        {
            Marshal.FreeHGlobal(dataPtr);
        }
    }

    private bool CallBackEnumWnd(IntPtr hwnd, IntPtr lParam)
    {
        var className = NativeMethods.GetClassName(hwnd);

        if (string.Equals(className, "IME", StringComparison.OrdinalIgnoreCase))
        {
            var data = Marshal.PtrToStructure<EnumWindowsData>(lParam);
            var rootOwner = NativeMethods.GetAncestor(hwnd, NativeMethods.GA_ROOTOWNER);

            if (data.ConsoleWindow == rootOwner)
            {
                data.FoundIMEWindow = hwnd;
                Marshal.StructureToPtr(data, lParam, false);

                return false;
            }
        }

        return true;
    }

    private nint[] GetKeyboardLayoutsUsingWinApi()
    {
        int count = NativeMethods.GetKeyboardLayoutList(0, null);
        var keyboardLayouts = new nint[count];
        NativeMethods.GetKeyboardLayoutList(keyboardLayouts.Length, keyboardLayouts);

        return keyboardLayouts;
    }

    private CultureInfo[] OrderKeyboardLayouts(nint[] loadedLayouts)
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\International\User Profile");

        if (key?.GetValue("Languages") is not string[] languageCodes)
            return [];

        var languageIndexes = languageCodes
            .Select((lang, i) => new { Lang = lang, Index = i })
            .ToDictionary(x => x.Lang, x => x.Index);

        return loadedLayouts
            .Select(ConvertToCultureInfo)
            .Distinct()
            .OrderBy(c =>
            {
                if (languageIndexes.TryGetValue(c.Name, out var exact))
                    return exact;

                if (languageIndexes.TryGetValue(c.TwoLetterISOLanguageName, out var fallback))
                    return fallback;

                return int.MaxValue;
            })
            .ToArray();
    }

    private CultureInfo ConvertToCultureInfo(nint hkl)
    {
        ushort langId = (ushort)((long)hkl & 0xFFFF);

        try
        {
            return new CultureInfo(langId);
        }
        catch (CultureNotFoundException)
        {
            _logger.LogWarning("Culture not found for HKL: {HKL:X}", hkl);
            return CultureInfo.InvariantCulture;
        }
    }

    private bool AreKeyAndModifierEqual(Key key, KeyModifier modifier)
    {
        return key.IsAlt && modifier == KeyModifier.Alt
            || key.IsCtrl && modifier == KeyModifier.Ctrl
            || key.IsShift && modifier == KeyModifier.Shift
            || key.IsWin && modifier == KeyModifier.Win;
    }
}
