using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using WinTool.Model;

namespace WinTool.Services
{
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
                var settings = JsonConvert.DeserializeObject<Settings>(json);
                return settings;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return null;
            }
        }

        public void UpdateSettings(Settings settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
