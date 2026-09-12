using System.Text.Json.Serialization;

namespace WinTool.Updater;

public record GitHubReleaseAsset(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("size")] long Size,
    [property: JsonPropertyName("digest")] string? Digest);
