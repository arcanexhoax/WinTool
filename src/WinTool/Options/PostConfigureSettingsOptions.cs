using Microsoft.Extensions.Options;
using System;
using WinTool.ViewModels.Settings;

namespace WinTool.Options;

public class PostConfigureSettingsOptions : IPostConfigureOptions<SettingsOptions>
{
    private static readonly string[] s_supportedLanguages = ["en", "de", "es", "fr", "pl", "pt", "ru", "tr", "uk"];

    public void PostConfigure(string? _, SettingsOptions o)
    {
        o.Language = ValidateValue(o.Language, s_supportedLanguages, App.SystemUICulture.TwoLetterISOLanguageName);
        o.AnimationMode = ValidateEnum(o.AnimationMode, AnimationMode.Auto);
        o.AppTheme = ValidateEnum(o.AppTheme, AppTheme.System);
    }

    private TEnum ValidateEnum<TEnum>(TEnum value, TEnum fallbackValue) where TEnum : struct, Enum
    {
        return Enum.IsDefined(value) ? value : fallbackValue;
    }

    private string ValidateValue(string? value, string[] allowedValues, string fallbackValue)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallbackValue;

        foreach (var allowedValue in allowedValues)
        {
            if (string.Equals(allowedValue, value, StringComparison.OrdinalIgnoreCase))
                return allowedValue;
        }

        return fallbackValue;
    }
}