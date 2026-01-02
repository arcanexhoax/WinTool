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
                    "AppTheme": "Dark"
                },
                "FeaturesOptions": { "EnableSwitchLanguagePopup": "False" },
                "ShortcutsOptions": { "Shortcuts": { "CreateFile": "Alt + F1" } }
            }
            """;
        _fileSystem.File.WriteAllText(_appSettingsPath, json);

        var sp = BuildServiceProvider(_appSettingsPath); 
        var settings = sp.GetRequiredService<IOptionsMonitor<SettingsOptions>>().CurrentValue;
        var features = sp.GetRequiredService<IOptionsMonitor<FeaturesOptions>>().CurrentValue;
        var shortcuts = sp.GetRequiredService<IOptionsMonitor<ShortcutsOptions>>().CurrentValue;

        Assert.False(settings.WindowsStartupEnabled);
        Assert.True(settings.AlwaysRunAsAdmin);
        Assert.Equal("Dark", settings.AppTheme.ToString());
        Assert.False(features.EnableSwitchLanguagePopup);
        Assert.Equal("Alt + F1", shortcuts.Shortcuts["CreateFile"]);
        Assert.Equal("Ctrl + Shift + E", shortcuts.Shortcuts["FastFileCreation"]);
    }

    [Fact]
    public void WritableOptions_CreatesFile()
    {
        var sp = BuildServiceProvider(_appSettingsPath);
        var shortcutsOptions = sp.GetRequiredService<WritableOptions<ShortcutsOptions>>();
        var featuresOptions = sp.GetRequiredService<WritableOptions<FeaturesOptions>>();
        var settingsOptions = sp.GetRequiredService<WritableOptions<SettingsOptions>>();

        shortcutsOptions.Update(o => o.Shortcuts["CreateFile"] = "Alt + F1");
        featuresOptions.Update(o => o.EnableSwitchLanguagePopup = false);
        settingsOptions.Update(o => o.AppTheme = ViewModels.Settings.AppTheme.Light);

        var text = _fileSystem.File.ReadAllText(_appSettingsPath);

        Assert.Contains("\"CreateFile\": \"Alt + F1\"", text);
        Assert.Contains("\"EnableSwitchLanguagePopup\": \"False\"", text);
        Assert.Contains("\"AppTheme\": \"1\"", text);
    }

    [Fact]
    public void WritableOptions_UpdatesWritesToFile()
    {
        var json = """
            {
                "SettingsOptions": { "AppTheme": "Dark" },
                "FeaturesOptions": { "EnableSwitchLanguagePopup": "False" },
                "ShortcutsOptions": { "Shortcuts": { "CreateFile": "Ctrl + Q" } }
            }
            """;
        _fileSystem.File.WriteAllText(_appSettingsPath, json);

        var sp = BuildServiceProvider(_appSettingsPath);
        var shortcutsOptions = sp.GetRequiredService<WritableOptions<ShortcutsOptions>>();
        var featuresOptions = sp.GetRequiredService<WritableOptions<FeaturesOptions>>();
        var settingsOptions = sp.GetRequiredService<WritableOptions<SettingsOptions>>();

        shortcutsOptions.Update(o => o.Shortcuts["CreateFile"] = "Alt + F1");
        featuresOptions.Update(o => o.EnableSwitchLanguagePopup = true);
        settingsOptions.Update(o => o.AppTheme = ViewModels.Settings.AppTheme.Light);

        var text = _fileSystem.File.ReadAllText(_appSettingsPath);

        Assert.Contains("\"CreateFile\": \"Alt + F1\"", text);
        Assert.Contains("\"EnableSwitchLanguagePopup\": \"True\"", text);
        Assert.Contains("\"AppTheme\": \"1\"", text);
    }

    [Fact]
    public void ShortcutOptions_ValidatesEmptySettings()
    {
        var sp = BuildServiceProvider(_appSettingsPath);
        var shortcutsOptions = sp.GetRequiredService<WritableOptions<ShortcutsOptions>>();

        Assert.Equal("Ctrl + Shift + Enter", shortcutsOptions.CurrentValue.Shortcuts["RunFileAsAdmin"]);
    }

    [Fact]
    public void ShortcutOptions_ValidatesInvalidAndDuplicatedShortcuts()
    {
        var json = """
            {
                "ShortcutsOptions": { 
                    "Shortcuts": { 
                        "CreateFile": "Ctrl + Shift + E",
                        "FastFileCreation": "ctrl+shift+e",
                        "RunFileWithArgs": "ctrl+p",
                        "OpenFolderInCmd": "abc",
                        "SelectedItemCopyName" : "A",
                        "SelectedItemCopyPath" : "Ctrl + Shift + Enter"
                    } 
                }
            }
            """;
        _fileSystem.File.WriteAllText(_appSettingsPath, json);

        var sp = BuildServiceProvider(_appSettingsPath);
        var shortcutsOptions = sp.GetRequiredService<WritableOptions<ShortcutsOptions>>();

        Assert.Equal("Ctrl + Shift + E", shortcutsOptions.CurrentValue.Shortcuts["CreateFile"]);
        Assert.Null(shortcutsOptions.CurrentValue.Shortcuts["FastFileCreation"]);
        Assert.Equal("ctrl+p", shortcutsOptions.CurrentValue.Shortcuts["RunFileWithArgs"]);
        Assert.Null(shortcutsOptions.CurrentValue.Shortcuts["OpenFolderInCmd"]);
        Assert.Equal("Ctrl + Shift + X", shortcutsOptions.CurrentValue.Shortcuts["SelectedItemCopyName"]);
        Assert.Equal("Ctrl + Shift + Enter", shortcutsOptions.CurrentValue.Shortcuts["SelectedItemCopyPath"]);
        Assert.Null(shortcutsOptions.CurrentValue.Shortcuts["RunFileAsAdmin"]);
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

        services.AddSingleton(jsonOptions);
        services.AddSingleton(fileProvider);
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton<IOptionsMonitor<SettingsOptions>, OptionsMonitor<SettingsOptions>>();
        services.AddSingleton<IOptionsMonitor<FeaturesOptions>, OptionsMonitor<FeaturesOptions>>();
        services.AddSingleton<IOptionsMonitor<ShortcutsOptions>, OptionsMonitor<ShortcutsOptions>>();
        services.AddSingleton<IPostConfigureOptions<ShortcutsOptions>, PostConfigureShortcutsOptions>();
        services.AddSingleton<WritableOptions<SettingsOptions>>();
        services.AddSingleton<WritableOptions<FeaturesOptions>>();
        services.AddSingleton<WritableOptions<ShortcutsOptions>>();

        return services.BuildServiceProvider();
    }
}
