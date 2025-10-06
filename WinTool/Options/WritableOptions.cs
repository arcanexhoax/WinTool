using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using WinTool.Extensions;
using WinTool.Utils;

namespace WinTool.Options;

public class WritableOptions<T>(CustomFileConfigurationProvider provider, IOptionsMonitor<T> options, JsonSerializerOptions jsonOptions)
{
    private readonly CustomFileConfigurationProvider _provider = provider;
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions;

    public T CurrentValue => options.CurrentValue;

    public void Update(Action update)
    {
        update();
        var data = CurrentValue.ToDictionary(_jsonOptions);
        _provider.Set(data);
    }
}
