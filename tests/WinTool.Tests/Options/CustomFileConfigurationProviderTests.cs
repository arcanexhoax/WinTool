using System.IO.Abstractions.TestingHelpers;
using System.Text.Encodings.Web;
using System.Text.Json;
using WinTool.Extensions;
using WinTool.Options;

namespace WinTool.Tests.Options;

public class CustomFileConfigurationProviderTests
{
    private readonly string _configPath;
    private readonly MockFileSystem _fileSystem = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    public CustomFileConfigurationProviderTests()
    {
        var path = _fileSystem.Path;
        _configPath = path.Combine(path.GetTempPath(), path.GetRandomFileName());
    }

    [Fact]
    public void Load_MissingFile_ProducesEmptyConfiguration()
    {
        var provider = CreateProvider();

        provider.Load();

        Assert.False(provider.TryGet("SettingsOptions:Language", out _));
        Assert.False(provider.TryGet("ShortcutsOptions:Shortcuts:CreateFile", out _));
    }

    [Fact]
    public void Load_ExistingNestedJson_FlattensConfigurationKeys()
    {
        var json = """
            {
                "SettingsOptions": {
                    "Language": "uk",
                    "Recent": ["one", "two"]
                },
                "ShortcutsOptions": {
                    "Shortcuts": {
                        "CreateFile": "Ctrl + E"
                    }
                }
            }
            """;
        _fileSystem.File.WriteAllText(_configPath, json);

        var provider = CreateProvider();
        provider.Load();

        Assert.True(provider.TryGet("SettingsOptions:Language", out var language));
        Assert.Equal("uk", language);
        Assert.True(provider.TryGet("SettingsOptions:Recent:1", out var secondRecent));
        Assert.Equal("two", secondRecent);
        Assert.True(provider.TryGet("ShortcutsOptions:Shortcuts:CreateFile", out var createFileShortcut));
        Assert.Equal("Ctrl + E", createFileShortcut);
    }

    [Fact]
    public void Set_UpdatesExistingConfiguration_AndPersistsMergedJson()
    {
        var json = """
            {
                "SettingsOptions": {
                    "Language": "en",
                    "WindowsStartupEnabled": true
                },
                "ShortcutsOptions": {
                    "Shortcuts": {
                        "CreateFile": "Ctrl + E"
                    }
                }
            }
            """;
        _fileSystem.File.WriteAllText(_configPath, json);

        var provider = CreateProvider();
        provider.Load();

        provider.Set(new Dictionary<string, object?>
        {
            ["SettingsOptions:Language"] = "uk",
            ["SettingsOptions:AlwaysRunAsAdmin"] = true
        });

        Assert.True(provider.TryGet("SettingsOptions:Language", out var language));
        Assert.Equal("uk", language);

        var persisted = JsonSerializer.Deserialize<Dictionary<string, object?>>(_fileSystem.File.ReadAllText(_configPath))!.Flatten();

        Assert.Equal("uk", persisted["SettingsOptions:Language"]);
        Assert.Equal("true", persisted["SettingsOptions:AlwaysRunAsAdmin"]);
        Assert.Equal("True", persisted["SettingsOptions:WindowsStartupEnabled"]);
        Assert.Equal("Ctrl + E", persisted["ShortcutsOptions:Shortcuts:CreateFile"]);
    }

    private CustomFileConfigurationProvider CreateProvider()
    {
        return new CustomFileConfigurationProvider(_configPath, _jsonOptions, _fileSystem);
    }
}