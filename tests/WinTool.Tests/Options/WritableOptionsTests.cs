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
        var content = GenerateAppSettings();

        _appSettingsPath = path.Combine(path.GetTempPath(), path.GetRandomFileName());

        _fileSystem.File.WriteAllText(_appSettingsPath, content);
    }

    [Fact]
    public void AddConfigurationFileProvider_LoadsOptionsCorrectly()
    {
        var sp = BuildServiceProvider(_appSettingsPath); 
        var settings = sp.GetRequiredService<IOptionsMonitor<SettingsOptions>>().CurrentValue;
        var features = sp.GetRequiredService<IOptionsMonitor<FeaturesOptions>>().CurrentValue;
        var shortcuts = sp.GetRequiredService<IOptionsMonitor<ShortcutsOptions>>().CurrentValue;

        Assert.True(settings.WindowsStartupEnabled);
        Assert.False(settings.AlwaysRunAsAdmin);
        Assert.Equal("Dark", settings.AppTheme.ToString());
        Assert.True(features.EnableSwitchLanguagePopup);
        Assert.Equal("Ctrl + E", shortcuts.Shortcuts["CreateFile"]);
    }

    [Fact]
    public void WritableOptions_Update_SettingsOptions_WritesToFile()
    {
        var sp = BuildServiceProvider(_appSettingsPath);
        var writable = sp.GetRequiredService<WritableOptions<SettingsOptions>>();

        writable.Update(o => o.AlwaysRunAsAdmin = true);

        var text = _fileSystem.File.ReadAllText(_appSettingsPath);
        Assert.Contains("\"AlwaysRunAsAdmin\": \"True\"", text);
    }

    [Fact]
    public void WritableOptions_Update_FeaturesOptions_WritesToFile()
    {
        var sp = BuildServiceProvider(_appSettingsPath);
        var writable = sp.GetRequiredService<WritableOptions<FeaturesOptions>>();

        writable.Update(o => o.EnableSwitchLanguagePopup = false);

        var text = _fileSystem.File.ReadAllText(_appSettingsPath);
        Assert.Contains("\"EnableSwitchLanguagePopup\": \"False\"", text);
    }

    [Fact]
    public void WritableOptions_Update_ShortcutsOptions_WritesToFile()
    {
        var sp = BuildServiceProvider(_appSettingsPath);
        var writable = sp.GetRequiredService<WritableOptions<ShortcutsOptions>>();

        writable.Update(o => o.Shortcuts["CreateFile"] = "Alt + F1");

        var text = _fileSystem.File.ReadAllText(_appSettingsPath);
        Assert.Contains("\"CreateFile\": \"Alt + F1\"", text);
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
        services.AddSingleton<WritableOptions<SettingsOptions>>();
        services.AddSingleton<WritableOptions<FeaturesOptions>>();
        services.AddSingleton<WritableOptions<ShortcutsOptions>>();

        return services.BuildServiceProvider();
    }

    private string GenerateAppSettings()
    {
        return 
            """
            {
                "SettingsOptions": {
                    "WindowsStartupEnabled": true,
                    "AlwaysRunAsAdmin": false,
                    "AppTheme": "Dark"
                },
                "FeaturesOptions": {
                    "EnableSwitchLanguagePopup": true
                },
                "ShortcutsOptions": {
                    "Shortcuts": {
                        "CreateFile": "Ctrl + E",
                        "FastFileCreation": "Ctrl + Shift + E"
                    }
                }
            }
            """;
    }
}
