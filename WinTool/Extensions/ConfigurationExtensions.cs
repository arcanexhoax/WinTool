using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using WinTool.Utils;

namespace WinTool.Extensions;

public static class ConfigurationExtensions
{
    public static IHostApplicationBuilder AddConfigurationFileProvider(this IHostApplicationBuilder builder)
    {
        string appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WinTool");

        if (!Directory.Exists(appFolderPath))
            Directory.CreateDirectory(appFolderPath);

        var filePath = Path.Combine(appFolderPath, "appsettings.json");
        var fileProvider = new CustomFileConfigurationProvider(filePath);

        builder.Services.AddSingleton(fileProvider);
        builder.Configuration.Add(new CustomFileConfigurationSource(fileProvider));

        return builder;
    }
}
