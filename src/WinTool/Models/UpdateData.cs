using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WinTool.Models;

public record GitHubRelease(
    [property: JsonPropertyName("tag_name")] string? TagName,
    [property: JsonPropertyName("html_url")] Uri? ReleaseUri,
    [property: JsonPropertyName("assets")] List<GitHubReleaseAsset>? Assets);

public record GitHubReleaseAsset(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("browser_download_url")] Uri? DownloadUri,
    [property: JsonPropertyName("size")] long Size,
    [property: JsonPropertyName("id")] long Id = 0,
    [property: JsonPropertyName("digest")] string? Digest = null);

public record UpdateCheckResult(bool IsUpdateAvailable, Version LatestVersion, Uri ReleaseUri, GitHubReleaseAsset? Asset);

public record UpdateStateInfo(UpdateState State, UpdateCheckResult? Result = null, Exception? Error = null);

public readonly record struct UpdateDownloadProgress(long BytesReceived, long TotalBytes)
{
    public double Percentage => TotalBytes > 0 ? BytesReceived * 100d / TotalBytes : 0;
}

public enum UpdateState
{
    NotChecked,
    Checking,
    UpToDate,
    Available,
    Downloading,
    Installing,
    Error
}
