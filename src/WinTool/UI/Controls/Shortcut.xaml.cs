using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WinTool.UI.Controls;

public class Shortcut : Control
{
    private static readonly DependencyPropertyKey TokensPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Tokens),
        typeof(IReadOnlyList<ShortcutToken>),
        typeof(Shortcut),
        new PropertyMetadata(Array.Empty<ShortcutToken>()));

    public static readonly DependencyProperty TokensProperty = TokensPropertyKey.DependencyProperty;

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(GlobalKeyInterceptor.Shortcut),
        typeof(Shortcut),
        new PropertyMetadata(null, OnValueChanged));

    static Shortcut()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Shortcut),
            new FrameworkPropertyMetadata(typeof(Shortcut)));
    }

    public GlobalKeyInterceptor.Shortcut? Value
    {
        get => (GlobalKeyInterceptor.Shortcut?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IReadOnlyList<ShortcutToken> Tokens
    {
        get => (IReadOnlyList<ShortcutToken>)GetValue(TokensProperty);
        private set => SetValue(TokensPropertyKey, value);
    }

    private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        ((Shortcut)dependencyObject).Tokens = BuildTokens((GlobalKeyInterceptor.Shortcut?)e.NewValue);
    }

    private static List<ShortcutToken> BuildTokens(GlobalKeyInterceptor.Shortcut? shortcut)
    {
        if (shortcut is null)
            return [];

        List<ShortcutToken> tokens = [];

        AddModifierToken(tokens, shortcut.Modifier, KeyModifier.Win);
        AddModifierToken(tokens, shortcut.Modifier, KeyModifier.Ctrl);
        AddModifierToken(tokens, shortcut.Modifier, KeyModifier.Shift);
        AddModifierToken(tokens, shortcut.Modifier, KeyModifier.Alt);

        var keyText = shortcut.Key.ToFormattedString();
        tokens.Add(new ShortcutToken(keyText));

        return tokens;
    }

    private static void AddModifierToken(List<ShortcutToken> tokens, KeyModifier value, KeyModifier modifier)
    {
        if ((value & modifier) == modifier)
        {
            var token = modifier switch
            {
                KeyModifier.Win => new ShortcutToken(IsWindowsKey: true),
                _ => new ShortcutToken(modifier.ToString()),
            };

            tokens.Add(token);
        }
    }
}

public sealed record ShortcutToken(string? Text = null, bool IsWindowsKey = false);