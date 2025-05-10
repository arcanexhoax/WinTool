using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WinTool.Native;

namespace WinTool.Services
{
    public class KeyboardLayoutManager
    {
        private readonly KeyInterceptor _keyInterceptor;

        private nint _lastLayout;
        private IEnumerable<nint> _allLayouts;
        private CancellationTokenSource? _checkLayoutCts;
        private Shortcut? _waitingShortcut;
        private bool _waitingForWinRelease;

        public IEnumerable<CultureInfo> AllCultures => _allLayouts.Select(ConvertToCultureInfo);

        public event Action<CultureInfo>? LayoutChanged;
        public event Action<IEnumerable<CultureInfo>>? LayoutsListChanged;

        public KeyboardLayoutManager(KeyInterceptor keyInterceptor)
        {
            _keyInterceptor = keyInterceptor;

            _lastLayout = GetCurrentKeyboardLayout();
            _allLayouts = GetKeyboardLayouts();
        }

        private async void OnShortcutPressed(object? sender, ShortcutPressedEventArgs e)
        {
            // User can switch keyboard layout with Ctrl + Shift/Shift + Ctrl/Alt + Shift/Shift + Alt
            // But if the shortcut is Shift + Alt and it is in Up state, Windows will send Shift + Ctrl (Up)
            // So we need to track the correct shortcut and check the current layout only after that
            if ((e.Shortcut.Key.IsAlt() && e.Shortcut.Modifier is KeyModifier.Shift
                || e.Shortcut.Key.IsShift() && e.Shortcut.Modifier is KeyModifier.Alt
                || e.Shortcut.Key.IsCtrl() && e.Shortcut.Modifier is KeyModifier.Shift
                || e.Shortcut.Key.IsShift() && e.Shortcut.Modifier is KeyModifier.Ctrl)
                && e.Shortcut.State is KeyState.Down)
            {
                _waitingShortcut = e.Shortcut;
            }
            else if (_waitingShortcut is not null
                && e.Shortcut.Key == _waitingShortcut.Key 
                && e.Shortcut.Modifier == _waitingShortcut.Modifier
                && e.Shortcut.State is KeyState.Up)
            {
                _checkLayoutCts?.Cancel();
                _checkLayoutCts = new CancellationTokenSource();
                await CheckLayoutAsync(_checkLayoutCts.Token);
            }
            // The second way to change the layout is Win + Space
            // When the user clicking Space while Win is pressed, the current layout does NOT change
            // The layout will only change after releasing Win (not in all cases, but it's ok)
            else if (e.Shortcut is { Key: Key.Space, Modifier: KeyModifier.Win, State: KeyState.Down })
            {
                _waitingForWinRelease = true;
            }
            else if (e.Shortcut.Key.IsWin() && e.Shortcut.State == KeyState.Up && _waitingForWinRelease)
            {
                _waitingForWinRelease = false;

                _checkLayoutCts?.Cancel();
                _checkLayoutCts = new CancellationTokenSource();
                await CheckLayoutAsync(_checkLayoutCts.Token);
            }
        }

        public void Start()
        {
            _keyInterceptor.ShortcutPressed += OnShortcutPressed;
        }

        public void Stop()
        {
            _checkLayoutCts?.Cancel();
            _keyInterceptor.ShortcutPressed -= OnShortcutPressed;
        }

        private async Task CheckLayoutAsync(CancellationToken cancellationToken)
        {
            try
            {
                for (int i = 0; i < 20; i++)
                {
                    await Task.Delay(20, cancellationToken);

                    var currentLayout = GetCurrentKeyboardLayout();

                    if (currentLayout != _lastLayout)
                    {
                        _lastLayout = currentLayout;
                        var allLayouts = GetKeyboardLayouts();

                        if (!_allLayouts.SequenceEqual(allLayouts))
                        {
                            _allLayouts = allLayouts;
                            LayoutsListChanged?.Invoke(allLayouts.Select(ConvertToCultureInfo));
                        }

                        var currentCulture = ConvertToCultureInfo(currentLayout);
                        Debug.WriteLine($"New layout: {currentCulture.NativeName}");
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
                Debug.WriteLine($"Error in keyboard layout cheking: {ex}");
            }
        }

        private nint GetCurrentKeyboardLayout()
        {
            var threadInfo = NativeMethods.GetGuiThreadInfo();

            if (threadInfo is null)
                return nint.Zero;

            var activeThreadId = NativeMethods.GetWindowThreadProcessId(threadInfo.Value.hwndFocus, out _);
            var currentLayout = NativeMethods.GetKeyboardLayout(activeThreadId);

            return currentLayout;
        }

        private nint[] GetKeyboardLayouts()
        {
            int count = NativeMethods.GetKeyboardLayoutList(0, null);
            var keyboardLayouts = new nint[count];
            NativeMethods.GetKeyboardLayoutList(keyboardLayouts.Length, keyboardLayouts);

            return keyboardLayouts;
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
                return CultureInfo.InvariantCulture;
            }
        }
    }
}
