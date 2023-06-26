using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WinTool.Enum;
using WinTool.Model;

namespace WinTool.Native
{
    public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    public class KeyHookerNative : IDisposable
    {
        public const int WH_KEYBOARD_LL = 13;
        public const int VkLeftShift = 0xA0;
        public const int VkRightShift = 0xA1;
        public const int VkLeftCtrl = 0xA2;
        public const int VkRightCtrl = 0xA3;
        public const int VkAlt = 0x2000;
        public const int LlkhfAltdown = (VkAlt >> 8);

        private IntPtr _windowsHookHandle;
        private IntPtr _user32LibraryHandle;
        private HookProc _hookProc;

        public event EventHandler<NativeKeyHookedEventArgs>? KeyPressed;

        public KeyHookerNative()
        {
            _windowsHookHandle = IntPtr.Zero;
            _user32LibraryHandle = IntPtr.Zero;
            _hookProc = LowLevelKeyboardProc; // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behavior.

            _user32LibraryHandle = NativeMethods.LoadLibrary("User32");
            if (_user32LibraryHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }

            _windowsHookHandle = NativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, _user32LibraryHandle, 0);
            if (_windowsHookHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }
        }

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool fEatKeyStroke = false;
            var wparamTyped = wParam.ToInt32();

            if (System.Enum.IsDefined(typeof(KeyState), wparamTyped))
            {
                object? marshaledStruct = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));

                if (marshaledStruct is not null)
                {
                    LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)marshaledStruct;

                    var eventArguments = new NativeKeyHookedEventArgs(p, (KeyState)wparamTyped);

                    EventHandler<NativeKeyHookedEventArgs>? handler = KeyPressed;
                    handler?.Invoke(this, eventArguments);

                    fEatKeyStroke = eventArguments.Handled;
                }
            }

            return fEatKeyStroke ? (IntPtr)1 : NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                // because we can unhook only in the same thread, not in garbage collector thread
                if (_windowsHookHandle != IntPtr.Zero)
                {
                    if (!NativeMethods.UnhookWindowsHookEx(_windowsHookHandle))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode, $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                    }
                    _windowsHookHandle = IntPtr.Zero;

#pragma warning disable CS8601 // Possible null reference assignment.
                    _hookProc -= LowLevelKeyboardProc;
#pragma warning restore CS8601 // Possible null reference assignment.
                }
            }

            if (_user32LibraryHandle != IntPtr.Zero)
            {
                if (!NativeMethods.FreeLibrary(_user32LibraryHandle)) // reduces reference to library by 1.
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }
                _user32LibraryHandle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~KeyHookerNative()
        {
            Dispose(false);
        }
    }
}