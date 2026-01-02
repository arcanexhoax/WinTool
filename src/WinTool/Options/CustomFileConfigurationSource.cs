using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading;
using WinTool.Extensions;

namespace WinTool.Options;

public class CustomFileConfigurationSource(CustomFileConfigurationProvider provider) : IConfigurationSource
{
    private readonly CustomFileConfigurationProvider _provider = provider;

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return _provider;
    }
}

public class CustomFileConfigurationProvider(string filePath, JsonSerializerOptions jsonOptions, IFileSystem fileSystem) : ConfigurationProvider
{
    private readonly string _filePath = filePath;
    private readonly Lock _lock = new();
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions;
    private readonly IFile _file = fileSystem.File;

    public override void Load()
    {
        if (!_file.Exists(_filePath))
        {
            Data = new Dictionary<string, string?>();
            return;
        }

        var json = _file.ReadAllText(_filePath);
        var data = JsonSerializer.Deserialize<Dictionary<string, object?>>(json)!;

        if (data == null)
        {
            Data = new Dictionary<string, string?>();
            return;
        }

        Data = data.Flatten();
    }

    public void Set(IDictionary<string, object?> obj)
    {
        lock (_lock)
        {
            var flattenedData = obj.Flatten();

            foreach (var (key, value) in flattenedData)
            {
                Data[key] = value;
            }

            var data = Data.Unflatten();
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            _file.WriteAllText(_filePath, json);

            OnReload();
        }
    }
}

