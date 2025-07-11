using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WinTool.Extensions;
using WinTool.Utils;

namespace WinTool.Options;

public class WritableOptions<T>(CustomFileConfigurationProvider provider, IOptionsMonitor<T> options)
{
    private readonly CustomFileConfigurationProvider _provider = provider;

    public T Options => options.CurrentValue;

    public void Update()
    {
        var data = ToDictionary(Options);
        _provider.Set(data);
    }

    public Dictionary<string, object?> ToDictionary(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)!;

        return dict.ToDictionary(
            kv => $"{typeof(T).Name}:{kv.Key}",
            kv => GetValue(kv.Value)
        );
    }

    private object? GetValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                p => p.Name,
                p => GetValue(p.Value)),
            JsonValueKind.Array => element.EnumerateArray().Select(GetValue).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }
}
