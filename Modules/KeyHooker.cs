using System;
using System.Windows.Input;
using WinTool.Enum;
using WinTool.Model;
using WinTool.Native;

namespace WinTool.Modules
{
    internal class KeyHooker : IDisposable
    {
        private readonly KeyHookerNative _hooker;
        private readonly Key[] _hookingKeys;

        public event EventHandler<KeyHookedEventArgs>? KeyHooked;

        public KeyHooker(params Key[] hookingKeys)
        {
            _hooker = new KeyHookerNative();
            _hooker.KeyPressed += OnKeyPressed;
            _hookingKeys = hookingKeys;
        }

        private void OnKeyPressed(object? sender, NativeKeyHookedEventArgs e)
        {
            if (e.KeyboardState != KeyState.KeyDown)
                return;

            Key key = KeyInterop.KeyFromVirtualKey(e.KeyboardData.VirtualCode);

            foreach (var hook in _hookingKeys)
            {
                if (hook == key)
                {
                    bool ctrlPressed = NativeMethods.GetAsyncKeyState(KeyHookerNative.VkLeftCtrl) > 1 || 
                        NativeMethods.GetAsyncKeyState(KeyHookerNative.VkRightCtrl) > 1;
                    bool shiftPressed = NativeMethods.GetAsyncKeyState(KeyHookerNative.VkLeftShift) > 1 || 
                        NativeMethods.GetAsyncKeyState(KeyHookerNative.VkRightShift) > 1;

                    KeyHooked?.Invoke(this, new KeyHookedEventArgs(key, (ctrlPressed ? KeyModifier.Ctrl : KeyModifier.None) | (shiftPressed ? KeyModifier.Shift : KeyModifier.None)));
                    //e.Handled = true;
                    break;
                }
            }
        }

        public void Dispose()
        {
            _hooker?.Dispose();
        }
    }
}