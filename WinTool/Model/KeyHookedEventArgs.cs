using System;
using System.Windows.Input;
using WinTool.Enum;

namespace WinTool.Model
{
    public class KeyHookedEventArgs : EventArgs
    {
        public Key Key { get; }
        public KeyModifier Modifier { get; }

        public KeyHookedEventArgs(Key key, KeyModifier modifier)
        {
            Key = key;
            Modifier = modifier;
        }
    }
}
