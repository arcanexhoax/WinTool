using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Models;

namespace WinTool.Services;

public class UpdateService(HttpClient httpClient, IFileSystem fileSystem)
{
    private const string LatestReleaseUri = "https://api.github.com/repos/arcanexhoax/WinTool/releases/latest";
    private const string UpdaterFileName = "Updater.exe";

    private readonly IFileSystem _fileSystem = fileSystem;
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _downloadDirectory = fileSystem.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WinTool");

    public async Task<UpdateCheckResult> CheckForUpdateAsync(Version currentVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(currentVersion);

        using var request = new HttpRequestMessage(HttpMethod.Get, LatestReleaseUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("WinTool", currentVersion.ToString(3)));

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var release = await response.Content.ReadFromJsonAsync<GitHubRelease>(cancellationToken);
        var tagName = release?.TagName?.Trim().TrimStart('v', 'V');

        if (!Version.TryParse(tagName, out var latestVersion))
            throw new InvalidDataException("The latest GitHub release has an invalid version tag.");

        if (release?.ReleaseUri is not { IsAbsoluteUri: true } releaseUri)
            throw new InvalidDataException("The latest GitHub release has an invalid URL.");

        var isUpdateAvailable = latestVersion > currentVersion;
        GitHubReleaseAsset? asset = null;

        if (isUpdateAvailable)
        {
            var expectedAssetName = $"WinTool-{latestVersion.ToString(3)}.msi";
            asset = release.Assets?.FirstOrDefault(a => string.Equals(a.Name, expectedAssetName, StringComparison.OrdinalIgnoreCase));

            if (asset is not { DownloadUri.IsAbsoluteUri: true, Size: > 0 })
                throw new InvalidDataException($"The latest GitHub release does not contain {expectedAssetName}.");
        }

        return new UpdateCheckResult(isUpdateAvailable, latestVersion, releaseUri, asset);
    }

    public async Task<string> DownloadUpdateAsync(GitHubReleaseAsset asset, IProgress<UpdateDownloadProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (asset is not { DownloadUri.IsAbsoluteUri: true, Size: > 0, Name: not (null or []) })
            throw new ArgumentException("The update asset is invalid.", nameof(asset));

        _fileSystem.Directory.CreateDirectory(_downloadDirectory);

        var destinationPath = _fileSystem.Path.Combine(_downloadDirectory, asset.Name);
        var temporaryPath = destinationPath + ".download";

        if (_fileSystem.File.Exists(temporaryPath))
            _fileSystem.File.Delete(temporaryPath);

        try
        {
            using var response = await _httpClient.GetAsync(asset.DownloadUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = asset.Size;
            var bytesReceived = 0L;
            var buffer = new byte[81920];

            progress?.Report(new UpdateDownloadProgress(0, totalBytes));

            await using (var source = await response.Content.ReadAsStreamAsync(cancellationToken))
            await using (var destination = _fileSystem.File.Open(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                int bytesRead;

                while ((bytesRead = await source.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    bytesReceived += bytesRead;
                    progress?.Report(new UpdateDownloadProgress(bytesReceived, totalBytes));
                }
            }

            if (bytesReceived != totalBytes)
                throw new InvalidDataException("The downloaded update size does not match the release asset size.");

            _fileSystem.File.Move(temporaryPath, destinationPath, true);
            return destinationPath;
        }
        catch
        {
            if (_fileSystem.File.Exists(temporaryPath))
                _fileSystem.File.Delete(temporaryPath);

            throw;
        }
    }

    public void StartUpdate(string installerPath, bool isBackground)
    {
        var applicationPath = Environment.ProcessPath ?? throw new InvalidOperationException("Unable to determine the WinTool path.");
        var sourceUpdaterPath = _fileSystem.Path.Combine(AppContext.BaseDirectory, UpdaterFileName);

        if (!_fileSystem.File.Exists(sourceUpdaterPath))
            throw new FileNotFoundException("The updater was not found.", sourceUpdaterPath);

        _fileSystem.Directory.CreateDirectory(_downloadDirectory);

        var targetUpdaterPath = _fileSystem.Path.Combine(_downloadDirectory, UpdaterFileName);
        _fileSystem.File.Copy(sourceUpdaterPath, targetUpdaterPath, true);

        var startInfo = new ProcessStartInfo(targetUpdaterPath)
        {
            UseShellExecute = true,
            Verb = "runas"
        };
        startInfo.ArgumentList.Add(Environment.ProcessId.ToString());
        startInfo.ArgumentList.Add(installerPath);
        startInfo.ArgumentList.Add(applicationPath);

        if (isBackground)
            startInfo.ArgumentList.Add(BackgroundParameter.ParameterName);

        using var updater = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start the updater.");
        Application.Current.Shutdown();
    }
}
