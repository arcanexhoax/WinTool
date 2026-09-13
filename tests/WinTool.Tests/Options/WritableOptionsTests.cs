using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Encodings.Web;
using System.Text.Json;
using WinTool.Options;

namespace WinTool.Tests.Options;

public class WritableOptionsTests
{
    private readonly string _appSettingsPath;
    private readonly MockFileSystem _fileSystem = new();

    public WritableOptionsTests()
    {
        var path = _fileSystem.Path;
        _appSettingsPath = path.Combine(path.GetTempPath(), path.GetRandomFileName());
    }

    [Fact]
    public void ConfigurationFileProvider_LoadsOptionsCorrectly()
    {
        var json = """
            {
                "SettingsOptions": {
                    "WindowsStartupEnabled": "False",
                    "AlwaysRunAsAdmin": "True",
                    "AppTheme": "Dark",
                    "AnimationMode": "On",
                    "Language": "uk",
                    "Notifications": { "NewVersions": "False" }
                },
                "FeaturesOptions": { "EnableInputPopup": "False" },
                "ShortcutsOptions": { "Shortcuts": { "CreateFile": "Alt + F1" } },
                "UpdateOptions": {
                    "InitialCheckDelay": "00:00:05",
                    "CheckInterval": "1.00:00:00",
                    "AvailableVersion": "1.2.3"
                }
            }
            """;
        _fileSystem.File.WriteAllText(_appSettingsPath, json);

        var sp = BuildServiceProvider(_appSettingsPath); 
        var settings = sp.GetRequiredService<IOptionsMonitor<SettingsOptions>>().CurrentValue;
        var features = sp.GetRequiredService<IOptionsMonitor<FeaturesOptions>>().CurrentValue;
        var shortcuts = sp.GetRequiredService<IOptionsMonitor<ShortcutsOptions>>().CurrentValue;
        var update = sp.GetRequiredService<IOptionsMonitor<UpdateOptions>>().CurrentValue;

        Assert.False(settings.WindowsStartupEnabled);
        Assert.True(settings.AlwaysRunAsAdmin);
        Assert.Equal("Dark", settings.AppTheme.ToString());
        Assert.Equal("On", settings.AnimationMode.ToString());
        Assert.Equal("uk", settings.Language);
        Assert.False(settings.Notifications.NewVersions);
        Assert.False(features.EnableInputPopup);
        Assert.Equal("Alt + F1", shortcuts.Shortcuts["CreateFile"]);
        Assert.Equal("Ctrl + Shift + C", shortcuts.Shortcuts["SelectedItemCopyPath"]);
        Assert.Equal(TimeSpan.FromSeconds(5), update.InitialCheckDelay);
        Assert.Equal(TimeSpan.FromDays(1), update.CheckInterval);
        Assert.Equal(new Version(1, 2, 3), update.AvailableVersion);
    }

    [Fact]
    public void WritableOptions_CreatesFile()
    {
        var sp = BuildServiceProvider(_appSettingsPath);
        var shortcutsOptions = sp.GetRequiredService<WritableOptions<ShortcutsOptions>>();
        var featuresOptions = sp.GetRequiredService<WritableOptions<FeaturesOptions>>();
        var settingsOptions = sp.GetRequiredService<WritableOptions<SettingsOptions>>();
        var updateOptions = sp.GetRequiredService<WritableOptions<UpdateOptions>>();

        shortcutsOptions.Update(o => o.Shortcuts["CreateFile"] = "Alt + F1");
        featuresOptions.Update(o => o.EnableInputPopup = false);
        settingsOptions.Update(o => o.AppTheme = WinTool.ViewModels.Settings.AppTheme.Light);
        settingsOptions.Update(o => o.Notifications.NewVersions = false);
        updateOptions.Update(o => o.AvailableVersion = new Version(1, 2, 3));

        var text = _fileSystem.File.ReadAllText(_appSettingsPath);

        Assert.Contains("\"CreateFile\": \"Alt + F1\"", text);
        Assert.Contains("\"EnableInputPopup\": \"False\"", text);
        Assert.Contains("\"AppTheme\": \"1\"", text);
        Assert.Contains("\"NewVersions\": \"False\"", text);
        Assert.Contains("\"AvailableVersion\": \"1.2.3\"", text);
    }

    [Fact]
    public void WritableOptions_UpdatesWritesToFile()
    {
        var json = """
            {
                "SettingsOptions": { "AppTheme": "Dark", "Notifications": { "NewVersions": "True" } },
                "FeaturesOptions": { "EnableInputPopup": "False" },
                "ShortcutsOptions": { "Shortcuts": { "CreateFile": "Ctrl + Q" } },
                "UpdateOptions": { "AvailableVersion": "1.0.0" }
            }
            """;
        _fileSystem.File.WriteAllText(_appSettingsPath, json);

        var sp = BuildServiceProvider(_appSettingsPath);
        var shortcutsOptions = sp.GetRequiredService<WritableOptions<ShortcutsOptions>>();
        var featuresOptions = sp.GetRequiredService<WritableOptions<FeaturesOptions>>();
        var settingsOptions = sp.GetRequiredService<WritableOptions<SettingsOptions>>();
        var updateOptions = sp.GetRequiredService<WritableOptions<UpdateOptions>>();

        shortcutsOptions.Update(o => o.Shortcuts["CreateFile"] = "Alt + F1");
        featuresOptions.Update(o => o.EnableInputPopup = true);
        settingsOptions.Update(o => o.AppTheme = WinTool.ViewModels.Settings.AppTheme.Light);
        settingsOptions.Update(o => o.Notifications.NewVersions = false);
        updateOptions.Update(o => o.AvailableVersion = new Version(1, 2, 3));

        var text = _fileSystem.File.ReadAllText(_appSettingsPath);

        Assert.Contains("\"CreateFile\": \"Alt + F1\"", text);
        Assert.Contains("\"EnableInputPopup\": \"True\"", text);
        Assert.Contains("\"AppTheme\": \"1\"", text);
        Assert.Contains("\"NewVersions\": \"False\"", text);
        Assert.Contains("\"AvailableVersion\": \"1.2.3\"", text);
    }

    [Fact]
    public void ShortcutOptions_ValidatesEmptySettings()
    {
        var sp = BuildServiceProvider(_appSettingsPath);
        var shortcutsOptions = sp.GetRequiredService<WritableOptions<ShortcutsOptions>>();

        Assert.Equal("Ctrl + Shift + StandardEnter", shortcutsOptions.CurrentValue.Shortcuts["RunFileAsAdmin"]);
    }

    [Fact]
    public void ShortcutOptions_ValidatesInvalidAndDuplicatedShortcuts()
    {
        var json = """
            {
                "ShortcutsOptions": { 
                    "Shortcuts": { 
                        "CreateFile": "Ctrl + Shift + P",
                        "RunFileAsAdmin": "ctrl+shift+p",
                        "RunFileWithArgs": "ctrl+p",
                        "OpenFolderInCmd": "abc",
                        "SelectedItemCopyName" : "A",
                        "SelectedItemCopyPath" : "Ctrl + Shift + StandardEnter",
                        "NonExistentShortcut": "Ctrl + Shift + F13"
                    } 
                }
            }
            """;
        _fileSystem.File.WriteAllText(_appSettingsPath, json);

        var sp = BuildServiceProvider(_appSettingsPath);
        var shortcutsOptions = sp.GetRequiredService<WritableOptions<ShortcutsOptions>>();

        Assert.Equal("Ctrl + Shift + P", shortcutsOptions.CurrentValue.Shortcuts["CreateFile"]);
        Assert.Null(shortcutsOptions.CurrentValue.Shortcuts["RunFileAsAdmin"]);
        Assert.Equal("ctrl+p", shortcutsOptions.CurrentValue.Shortcuts["RunFileWithArgs"]);
        Assert.Null(shortcutsOptions.CurrentValue.Shortcuts["OpenFolderInCmd"]);
        Assert.Equal("Ctrl + Shift + X", shortcutsOptions.CurrentValue.Shortcuts["SelectedItemCopyName"]);
        Assert.Equal("Ctrl + Shift + StandardEnter", shortcutsOptions.CurrentValue.Shortcuts["SelectedItemCopyPath"]);
        Assert.Null(shortcutsOptions.CurrentValue.Shortcuts["RunFileAsAdmin"]);
        Assert.DoesNotContain("NonExistentShortcut", shortcutsOptions.CurrentValue.Shortcuts);
    }

    [Fact]
    public void SettingsOptions_ValidatesInvalidValues()
    {
        var json = """
            {
                "SettingsOptions": {
                    "AppTheme": "999",
                    "AnimationMode": "999",
                    "Language": "invalid"
                }
            }
            """;
        _fileSystem.File.WriteAllText(_appSettingsPath, json);

        var sp = BuildServiceProvider(_appSettingsPath);
        var settings = sp.GetRequiredService<WritableOptions<SettingsOptions>>().CurrentValue;

        Assert.Equal(WinTool.ViewModels.Settings.AppTheme.System, settings.AppTheme);
        Assert.Equal(WinTool.ViewModels.Settings.AnimationMode.Auto, settings.AnimationMode);
        Assert.Equal(App.SystemUICulture.TwoLetterISOLanguageName, settings.Language);
    }

    [Fact]
    public void SettingsOptions_UsesSystemLanguageWhenMissing()
    {
        var sp = BuildServiceProvider(_appSettingsPath);
        var settings = sp.GetRequiredService<WritableOptions<SettingsOptions>>().CurrentValue;

        Assert.Equal(App.SystemUICulture.TwoLetterISOLanguageName, settings.Language);
    }

    private ServiceProvider BuildServiceProvider(string jsonFile)
    {
        var services = new ServiceCollection();
        var jsonOptions = new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true };
        var fileProvider = new CustomFileConfigurationProvider(jsonFile, jsonOptions, _fileSystem);
        var configBuilder = new ConfigurationBuilder();

        configBuilder.Add(new CustomFileConfigurationSource(fileProvider));
        var config = configBuilder.Build();

        services.Configure<SettingsOptions>(config.GetSection(nameof(SettingsOptions)));
        services.Configure<FeaturesOptions>(config.GetSection(nameof(FeaturesOptions)));
        services.Configure<ShortcutsOptions>(config.GetSection(nameof(ShortcutsOptions)));
        services.Configure<UpdateOptions>(config.GetSection(nameof(UpdateOptions)));

        services.AddSingleton(jsonOptions);
        services.AddSingleton(fileProvider);
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<IOptionsMonitor<SettingsOptions>, OptionsMonitor<SettingsOptions>>();
        services.AddSingleton<IOptionsMonitor<FeaturesOptions>, OptionsMonitor<FeaturesOptions>>();
        services.AddSingleton<IOptionsMonitor<ShortcutsOptions>, OptionsMonitor<ShortcutsOptions>>();
        services.AddSingleton<IOptionsMonitor<UpdateOptions>, OptionsMonitor<UpdateOptions>>();
        services.AddSingleton<IPostConfigureOptions<SettingsOptions>, PostConfigureSettingsOptions>();
        services.AddSingleton<IPostConfigureOptions<ShortcutsOptions>, PostConfigureShortcutsOptions>();
        services.AddSingleton<WritableOptions<SettingsOptions>>();
        services.AddSingleton<WritableOptions<FeaturesOptions>>();
        services.AddSingleton<WritableOptions<ShortcutsOptions>>();
        services.AddSingleton<WritableOptions<UpdateOptions>>();

        return services.BuildServiceProvider();
    }
}
