using System.Text.Encodings.Web;
using System.Text.Json;
using WinTool.Extensions;
using WinTool.Options;
using WinTool.ViewModels.Settings;

namespace WinTool.Tests.Extensions;

public class DictionaryExtensionsTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    [Fact]
    public void Flatten_JsonElementData_ProducesFlatInvariantDictionary()
    {
        var json = """
            {
                "SettingsOptions": {
                    "WindowsStartupEnabled": true,
                    "AppTheme": 2,
                    "Language": "uk",
                    "Recent": ["one", "two"],
                    "Nested": {
                        "Enabled": false,
                        "Created": "2026-04-04T12:34:56.0000000Z"
                    }
                }
            }
            """;
        var data = JsonSerializer.Deserialize<Dictionary<string, object?>>(json)!;

        var flattened = data.Flatten();

        Assert.Equal("True", flattened["SettingsOptions:WindowsStartupEnabled"]);
        Assert.Equal("2", flattened["SettingsOptions:AppTheme"]);
        Assert.Equal("uk", flattened["SettingsOptions:Language"]);
        Assert.Equal("one", flattened["SettingsOptions:Recent:0"]);
        Assert.Equal("two", flattened["SettingsOptions:Recent:1"]);
        Assert.Equal("False", flattened["SettingsOptions:Nested:Enabled"]);
        Assert.Equal("2026-04-04T12:34:56.0000000Z", flattened["SettingsOptions:Nested:Created"]);
    }

    [Fact]
    public void Unflatten_NestedKeysWithArrays_RehydratesHierarchy()
    {
        Dictionary<string, string?> flatData = new()
        {
            ["SettingsOptions:Language"] = "uk",
            ["SettingsOptions:Recent:0"] = "one",
            ["SettingsOptions:Recent:1"] = "two",
            ["SettingsOptions:Nested:Enabled"] = "false"
        };

        var data = flatData.Unflatten();

        var settings = Assert.IsType<Dictionary<string, object?>>(data["SettingsOptions"]);
        Assert.Equal("uk", settings["Language"]);

        var recent = Assert.IsType<object?[]>(settings["Recent"]);
        Assert.Collection(
            recent,
            item => Assert.Equal("one", item),
            item => Assert.Equal("two", item));

        var nested = Assert.IsType<Dictionary<string, object?>>(settings["Nested"]);
        Assert.Equal("false", nested["Enabled"]);
    }

    [Fact]
    public void ToDictionary_Object_UsesTypeNamePrefix()
    {
        var settings = new SettingsOptions
        {
            WindowsStartupEnabled = false,
            AlwaysRunAsAdmin = true,
            Language = "uk",
            AppTheme = AppTheme.Dark,
            AnimationMode = AnimationMode.Off
        };

        var data = settings.ToDictionary(_jsonOptions);

        Assert.Equal("False", data["SettingsOptions:WindowsStartupEnabled"]?.ToString());
        Assert.Equal("True", data["SettingsOptions:AlwaysRunAsAdmin"]?.ToString());
        Assert.Equal("uk", data["SettingsOptions:Language"]?.ToString());
        Assert.Equal(((int)AppTheme.Dark).ToString(), data["SettingsOptions:AppTheme"]?.ToString());
        Assert.Equal(((int)AnimationMode.Off).ToString(), data["SettingsOptions:AnimationMode"]?.ToString());
    }
}