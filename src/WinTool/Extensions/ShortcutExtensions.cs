using GlobalKeyInterceptor;
using System;
using System.Linq;

namespace WinTool.Extensions;

public static class ShortcutExtensions
{
    extension(Shortcut)
    {
        public static bool IsValid(string shortcut) => Parse(shortcut) != null;

        public static Shortcut? Parse(string shortcut) => Parse(shortcut, KeyState.Up);

        public static Shortcut? Parse(string shortcut, KeyState state)
        {
            var parts = shortcut.ToLower().Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 0)
                return null;

            KeyModifier modifier = KeyModifier.None;

            foreach (var (i, part) in parts.Index())
            {
                if (i + 1 == parts.Length)
                {
                    Key? key = Enum.TryParse<Key>(part, true, out var parsedKey) ? parsedKey : null;
                    return key is null ? null : new Shortcut(key.Value, modifier, state);
                }

                modifier |= part switch
                {
                    "ctrl" => KeyModifier.Ctrl,
                    "shift" => KeyModifier.Shift,
                    "alt" => KeyModifier.Alt,
                    "win" => KeyModifier.Win,
                    _ => KeyModifier.None
                };
            }

            return null;
        }
    }
}
