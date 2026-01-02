using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Text.Encodings.Web;
using System.Text.Json;
using WinTool.Options;

namespace WinTool.Extensions;

public static class ConfigurationExtensions
{
    public static IHostApplicationBuilder AddConfigurationFileProvider(this IHostApplicationBuilder builder)
    {
        var appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WinTool");
        var fileSystem = new FileSystem();

        fileSystem.Directory.CreateDirectory(appFolderPath);

        var jsonOptions = new JsonSerializerOptions() 
        { 
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        var filePath = Path.Combine(appFolderPath, "appsettings.json");
        var fileProvider = new CustomFileConfigurationProvider(filePath, jsonOptions, fileSystem);

        builder.Services.AddSingleton(jsonOptions);
        builder.Services.AddSingleton(fileProvider);
        builder.Services.AddSingleton<IFileSystem>(fileSystem);
        builder.Configuration.Add(new CustomFileConfigurationSource(fileProvider));

        return builder;
    }
}
