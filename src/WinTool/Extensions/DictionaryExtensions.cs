using System;
using System.Collections;
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
        if (value is JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        foreach (var inner in Flatten($"{prefix}:{prop.Name}", prop.Value))
                            yield return inner;
                    }
                    break;
                case JsonValueKind.Array:
                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        foreach (var inner in Flatten($"{prefix}:{index}", item))
                            yield return inner;
                        index++;
                    }
                    break;
                default:
                    yield return new KeyValuePair<string, object?>(prefix, value);
                    break;
            }
        }
        else if (value is IEnumerable enumerable and not string)
        {
            int index = 0;

            foreach (var item in enumerable)
            {
                foreach (var inner in Flatten($"{prefix}:{index}", item))
                    yield return inner;

                index++;
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
                var key = keys[i];
                var nextKey = keys[i + 1];

                // Check if the next key is a numeric index (indicates array)
                if (int.TryParse(nextKey, out _))
                {
                    if (!current.TryGetValue(key, out object? value) || value is not List<object?>)
                        current[key] = new List<object?>();

                    var list = (List<object?>)current[key]!;
                    var nextIndex = int.Parse(nextKey);

                    // Ensure the list is large enough
                    while (list.Count <= nextIndex)
                        list.Add(null);

                    // If this is the last key pair, set the value
                    if (i == keys.Length - 2)
                    {
                        list[nextIndex] = kv.Value;
                        break;
                    }
                    else
                    {
                        // Check if we need to create a nested structure
                        if (list[nextIndex] is not Dictionary<string, object?>)
                            list[nextIndex] = new Dictionary<string, object?>();

                        current = (Dictionary<string, object?>)list[nextIndex]!;
                        i++; // Skip the numeric index key
                    }
                }
                else
                {
                    if (!current.TryGetValue(key, out object? value) || value is not Dictionary<string, object?>)
                        current[key] = new Dictionary<string, object?>();

                    current = (Dictionary<string, object?>)current[key]!;
                }
            }

            // Handle the final key if we haven't already set it
            if (!int.TryParse(keys[^1], out _))
                current[keys[^1]] = kv.Value;
        }

        return ConvertListsToArrays(result);
    }

    private static Dictionary<string, object?> ConvertListsToArrays(Dictionary<string, object?> dict)
    {
        var result = new Dictionary<string, object?>();

        foreach (var kv in dict)
        {
            result[kv.Key] = kv.Value switch
            {
                List<object?> list => list.Select(ConvertValue).ToArray(),
                Dictionary<string, object?> nestedDict => ConvertListsToArrays(nestedDict),
                _ => kv.Value
            };
        }

        return result;
    }

    private static object? ConvertValue(object? value)
    {
        return value switch
        {
            Dictionary<string, object?> dict => ConvertListsToArrays(dict),
            List<object?> list => list.Select(ConvertValue).ToArray(),
            _ => value
        };
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

    public static Dictionary<string, object?> ToDictionary<T>(this T obj, JsonSerializerOptions jsonOptions)
    {
        var json = JsonSerializer.Serialize(obj, jsonOptions);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(json)!;

        return dict.ToDictionary(
            kv => $"{typeof(T).Name}:{kv.Key}",
            kv => kv.Value
        );
    }
}
