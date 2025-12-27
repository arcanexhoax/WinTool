using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using WinTool.Utils;

namespace WinTool.Extensions;

public static class ConfigurationExtensions
{
    public static IHostApplicationBuilder AddConfigurationFileProvider(this IHostApplicationBuilder builder)
    {
        string appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WinTool");

        if (!Directory.Exists(appFolderPath))
            Directory.CreateDirectory(appFolderPath);

        var jsonOptions = new JsonSerializerOptions() 
        { 
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var filePath = Path.Combine(appFolderPath, "appsettings.json");
        var fileProvider = new CustomFileConfigurationProvider(filePath, jsonOptions);

        builder.Services.AddSingleton(jsonOptions);
        builder.Services.AddSingleton(fileProvider);
        builder.Configuration.Add(new CustomFileConfigurationSource(fileProvider));

        return builder;
    }
}
