using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WinTool.Extensions;
using WinTool.Utils;

namespace WinTool.Options;

public class WritableOptions<T>(CustomFileConfigurationProvider provider, IOptionsMonitor<T> options, JsonSerializerOptions jsonOptions)
{
    private readonly CustomFileConfigurationProvider _provider = provider;
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions;

    public T Value => options.CurrentValue;

    public void Update()
    {
        var data = ToDictionary(Value);
        _provider.Set(data);
    }

    private Dictionary<string, object?> ToDictionary(T obj)
    {
        var json = JsonSerializer.Serialize(obj, _jsonOptions);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json)!;

        return dict.ToDictionary(
            kv => $"{typeof(T).Name}:{kv.Key}",
            kv => kv.Value
        );
    }
}
