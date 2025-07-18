﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using WinTool.Model;
using WinTool.Properties;
using WinTool.Utils;

namespace WinTool.Services;

public class SettingsManager
{
    private readonly string _settingsFilePath;

    public SettingsManager()
    {
        string settingsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WinTool");

        if (!Directory.Exists(settingsFolderPath))
            Directory.CreateDirectory(settingsFolderPath);

        _settingsFilePath = Path.Combine(settingsFolderPath, "settings.json");
    }

    public Settings? GetSettings()
    {
        if (!File.Exists(_settingsFilePath))
            return null;

        try
        {
            string json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<Settings>(json);
            return settings;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Get settings failed: {ex.Message}");
            MessageBoxHelper.ShowError(string.Format(Resources.GetSettingsError, ex.Message));
            return null;
        }
    }

    public void UpdateSettings(Settings settings)
    {
        try
        {
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Update settings failed: {ex.Message}");
            MessageBoxHelper.ShowError(string.Format(Resources.UpdateSettingsError, ex.Message));
        }
    }
}
