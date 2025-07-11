using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace WinTool.Extensions;

public static class DictionaryExtensions
{
    public static Dictionary<string, string?> Flatten(this IDictionary<string, object?> dictionary)
    {
        return dictionary
            .SelectMany(kv => Flatten(kv.Key, kv.Value))
            .ToDictionary(k => k.Key, v => ConvertToInvariantString(v.Value));
    }

    private static IEnumerable<KeyValuePair<string, object?>> Flatten(string prefix, object? value)
    {
        if (value is JsonElement element && element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                foreach (var inner in Flatten($"{prefix}:{prop.Name}", prop.Value))
                    yield return inner;
            }
        }
        else
        {
            yield return new KeyValuePair<string, object?>(prefix, value);
        }
    }

    public static Dictionary<string, object?> Unflatten(this IDictionary<string, string?> flatData)
    {
        var result = new Dictionary<string, object?>();

        foreach (var kv in flatData)
        {
            var keys = kv.Key.Split(':');
            var current = result;

            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (!current.TryGetValue(keys[i], out object? value) || value is not Dictionary<string, object?>)
                    current[keys[i]] = new Dictionary<string, object?>();

                current = (Dictionary<string, object?>)current[keys[i]]!;
            }

            current[keys[^1]] = kv.Value;
        }

        return result;
    }

    private static string? ConvertToInvariantString(object? value)
    {
        return value switch
        {
            null => null,
            bool b => b.ToString().ToLowerInvariant(),
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }
}
