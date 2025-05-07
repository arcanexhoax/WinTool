using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using WinTool.Native;

namespace WinTool.Services
{
    public class KeyboardLayoutManager : IDisposable
    {
        private readonly PeriodicTimer _layoutPollingTimer = new(TimeSpan.FromMilliseconds(100));

        private nint _lastLayout;

        public event Action<CultureInfo>? LayoutChanged;

        public KeyboardLayoutManager()
        {
            _lastLayout = GetCurrentKeyboardLayout();
        }

        public async Task Start()
        {
            while (await _layoutPollingTimer.WaitForNextTickAsync())
            {
                var currentLayout = GetCurrentKeyboardLayout();

                if (currentLayout != _lastLayout)
                {
                    _lastLayout = currentLayout;
                    var culture = CultureInfo.GetCultureInfo((short)currentLayout.ToInt64());

                    Debug.WriteLine($"New layout: {culture.ThreeLetterISOLanguageName}");
                    LayoutChanged?.Invoke(culture);
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

        public void Dispose()
        {
            _layoutPollingTimer.Dispose();
        }
    }
}
