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
    public class KeyboardLayoutManager : IDisposable
    {
        private readonly PeriodicTimer _layoutPollingTimer = new(TimeSpan.FromMilliseconds(100));

        private nint _lastLayout;
        private IEnumerable<nint> _allLayouts;

        public IEnumerable<CultureInfo> AllCultures => _allLayouts.Select(ConvertToCultureInfo);

        public event Action<CultureInfo>? LayoutChanged;
        public event Action<IEnumerable<CultureInfo>>? LayoutsListChanged;

        public KeyboardLayoutManager()
        {
            _lastLayout = GetCurrentKeyboardLayout();
            _allLayouts = GetKeyboardLayouts();
        }

        public async Task Start()
        {
            while (await _layoutPollingTimer.WaitForNextTickAsync())
            {
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
                }
            }
        }

        private nint GetCurrentKeyboardLayout()
        {
            var foregroundWindow = NativeMethods.GetForegroundWindow();
            var threadId = NativeMethods.GetWindowThreadProcessId(foregroundWindow, out _);
            var currentLayout = NativeMethods.GetKeyboardLayout(threadId);

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

        public void Dispose()
        {
            _layoutPollingTimer.Dispose();
        }
    }
}
