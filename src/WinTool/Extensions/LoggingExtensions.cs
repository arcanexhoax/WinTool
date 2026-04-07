using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using System.IO;

namespace WinTool.Extensions;

public static class LoggingExtensions
{
    public static HostApplicationBuilder AddNLogConfiguration(this HostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);

        // TODO delete when installer is added
        using var stream = typeof(App).Assembly.GetManifestResourceStream("WinTool.nlog.config");
        using var reader = new StreamReader(stream!);
        NLog.LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(reader.ReadToEnd());

        builder.Logging.AddNLog(NLog.LogManager.Configuration);

        return builder;
    }
}
