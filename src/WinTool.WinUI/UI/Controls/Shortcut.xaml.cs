using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;

namespace WinTool.WinUI.UI.Controls;

public class Shortcut : Control
{
    public static readonly DependencyProperty TokensProperty = DependencyProperty.Register(nameof(Tokens), typeof(IReadOnlyList<ShortcutToken>), typeof(Shortcut), new PropertyMetadata(Array.Empty<ShortcutToken>()));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(GlobalKeyInterceptor.Shortcut), typeof(Shortcut), new PropertyMetadata(null, OnValueChanged));

    public GlobalKeyInterceptor.Shortcut? Value
    {
        get => (GlobalKeyInterceptor.Shortcut?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IReadOnlyList<ShortcutToken> Tokens
    {
        get => (IReadOnlyList<ShortcutToken>)GetValue(TokensProperty);
        private set => SetValue(TokensProperty, value);
    }

    public Shortcut()
    {
        DefaultStyleKey = typeof(Shortcut);
    }

    private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        var control = (Shortcut)dependencyObject;
        control.Tokens = control.BuildTokens((GlobalKeyInterceptor.Shortcut?)e.NewValue);
    }

    private List<ShortcutToken> BuildTokens(GlobalKeyInterceptor.Shortcut? shortcut)
    {
        if (shortcut is null)
            return [];

        List<ShortcutToken> tokens = [];

        AddModifierToken(tokens, shortcut.Modifier, KeyModifier.Win);
        AddModifierToken(tokens, shortcut.Modifier, KeyModifier.Ctrl);
        AddModifierToken(tokens, shortcut.Modifier, KeyModifier.Shift);
        AddModifierToken(tokens, shortcut.Modifier, KeyModifier.Alt);

        var keyText = shortcut.Key.ToFormattedString();
        tokens.Add(new ShortcutToken(this, keyText));

        return tokens;
    }

    private void AddModifierToken(List<ShortcutToken> tokens, KeyModifier value, KeyModifier modifier)
    {
        if ((value & modifier) == modifier)
        {
            var token = modifier switch
            {
                KeyModifier.Win => new ShortcutToken(this, isWindowsKey: true),
                _ => new ShortcutToken(this, modifier.ToString()),
            };

            tokens.Add(token);
        }
    }
}

[Bindable]
public class ShortcutToken(Shortcut owner, string? text = null, bool isWindowsKey = false)
{
    public Shortcut Owner { get; } = owner;

    public string? Text { get; } = text;

    public bool IsWindowsKey { get; } = isWindowsKey;

    public Visibility TextVisibility => IsWindowsKey ? Visibility.Collapsed : Visibility.Visible;

    public Visibility WindowsVisibility => IsWindowsKey ? Visibility.Visible : Visibility.Collapsed;
}
