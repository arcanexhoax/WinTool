using System.ComponentModel;
using WinTool.Enum;

namespace WinTool.Model
{
    public class NativeKeyHookedEventArgs : HandledEventArgs
    {
        public KeyState KeyboardState { get; private set; }
        public LowLevelKeyboardInputEvent KeyboardData { get; private set; }

        public NativeKeyHookedEventArgs(LowLevelKeyboardInputEvent keyboardData, KeyState keyboardState)
        {
            KeyboardData = keyboardData;
            KeyboardState = keyboardState;
        }
    }
}
