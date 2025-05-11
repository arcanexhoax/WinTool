using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using WinTool.Native;
using WinTool.Utils;
using Resource = WinTool.Resources.Localizations.Resources;

namespace WinTool.Services
{
    public class RegistryManager : IDisposable
    {
        private const string KeyboardLayoutsKey = @"Keyboard Layout\Preload";

        private AutoResetEvent? _eventTerminate;
        private Thread? _watcherThread;

        public long[] KeyboardLayouts { get; private set; } = [];

        public event Action<long[]>? KeyboardLayoutsChanged;

        public void Start()
        {
            KeyboardLayouts = GetKeyboardLayouts();

            _eventTerminate = new AutoResetEvent(false);
            _watcherThread = new Thread(Watch) { IsBackground = true };
            _watcherThread.Start();
        }

        public void Stop()
        {
            _eventTerminate?.Set();
            _watcherThread?.Join();
            _eventTerminate?.Dispose();
            _eventTerminate = null;
            _watcherThread = null;
        }

        private void Watch()
        {
            using var key = Registry.CurrentUser.OpenSubKey(KeyboardLayoutsKey, false);

            if (key == null) 
                return;

            while (!_eventTerminate!.WaitOne(0))
            {
                if (NativeMethods.RegNotifyChangeKeyValue(key.Handle, true, RegChangeNotifyFilter.Value | RegChangeNotifyFilter.Name, IntPtr.Zero, false) != 0)
                    continue;

                if (_eventTerminate.WaitOne(0)) 
                    break;

                var newLayouts = GetKeyboardLayouts();

                if (!KeyboardLayouts.SequenceEqual(newLayouts))
                {
                    KeyboardLayouts = newLayouts;
                    KeyboardLayoutsChanged?.Invoke(KeyboardLayouts);
                }
            }
        }

        public long[] GetKeyboardLayouts()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(KeyboardLayoutsKey, false);

                if (key == null)
                    return [];

                return key
                    .GetValueNames()
                    .Select(name => key.GetValue(name)?.ToString())
                    .Where(value => !string.IsNullOrEmpty(value))
                    .Select(value => Convert.ToInt64(value, 16))
                    .ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error monitor keyboard layouts registry: {ex.Message}");
                MessageBoxHelper.ShowError(string.Format(Resource.MonitorKeyboardLayoutsError, ex.Message));
                return [];
            }
        }

        public void Dispose()
        {
            Stop();
            KeyboardLayoutsChanged = null;
        }
    }

    [Flags]
    public enum RegChangeNotifyFilter
    {
        Name = 1,
        Attributes = 2,
        LastSet = 4,
        Security = 8,
        Value = 16
    }
}
