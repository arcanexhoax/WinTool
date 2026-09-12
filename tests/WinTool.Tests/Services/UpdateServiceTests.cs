using System.Net;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using WinTool.Models;
using WinTool.Services;

namespace WinTool.Tests.Services;

public class UpdateServiceTests
{
    [Fact]
    public async Task CheckForUpdateAsync_WithNewerRelease_ReturnsAvailable()
    {
        var currentVersion = new Version(1, 2, 3);
        var handler = new StubHttpMessageHandler(CreateReleaseResponse(new Version(1, 2, 4), true));
        var service = CreateService(handler, currentVersion);

        var result = await service.CheckForUpdateAsync();

        Assert.True(result.IsUpdateAvailable);
        Assert.Equal(new Version(1, 2, 4), result.LatestVersion);
        Assert.Equal("https://github.com/arcanexhoax/WinTool/releases/tag/v1.2.4", result.ReleaseUri.AbsoluteUri);
        Assert.Equal("WinTool/1.2.3", handler.Request?.Headers.UserAgent.ToString());
        Assert.Equal("WinTool-1.2.4.exe", result.Asset?.Name);
        Assert.Equal(42, result.Asset?.Id);
        Assert.Equal("sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef", result.Asset?.Digest);
    }

    [Fact]
    public async Task CheckForUpdateAsync_WithCurrentRelease_ReturnsUpToDate()
    {
        var handler = new StubHttpMessageHandler(CreateReleaseResponse(new Version(1, 2, 3), true));
        var service = CreateService(handler, new Version(1, 2, 3));

        var result = await service.CheckForUpdateAsync();

        Assert.False(result.IsUpdateAvailable);
        Assert.Null(result.Asset);
    }

    [Fact]
    public async Task CheckForUpdateAsync_WithOlderRelease_ReturnsUpToDate()
    {
        var handler = new StubHttpMessageHandler(CreateReleaseResponse(new Version(1, 2, 2), true));
        var service = CreateService(handler, new Version(1, 2, 3));

        var result = await service.CheckForUpdateAsync();

        Assert.False(result.IsUpdateAvailable);
        Assert.Null(result.Asset);
    }

    [Fact]
    public async Task CheckForUpdateAsync_WithInvalidTag_ThrowsInvalidDataException()
    {
        var handler = new StubHttpMessageHandler(CreateJsonResponse("""{"tag_name":"latest","html_url":"https://github.com/arcanexhoax/WinTool/releases/latest"}"""));
        var service = CreateService(handler);

        await Assert.ThrowsAsync<InvalidDataException>(() => service.CheckForUpdateAsync());
    }

    [Fact]
    public async Task CheckForUpdateAsync_WithoutInstallerAsset_ThrowsInvalidDataException()
    {
        var handler = new StubHttpMessageHandler(CreateReleaseResponse(new Version(1, 2, 4), false));
        var service = CreateService(handler, new Version(1, 2, 3));

        await Assert.ThrowsAsync<InvalidDataException>(() => service.CheckForUpdateAsync());
    }

    [Fact]
    public async Task DownloadUpdateAsync_DownloadsInstallerToWinToolDataDirectory()
    {
        byte[] installer = [1, 2, 3, 4];
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(installer),
        });
        var fileSystem = new MockFileSystem();
        var service = CreateService(handler, fileSystem: fileSystem);
        var asset = new GitHubReleaseAsset("WinTool-1.1.0.exe", new Uri("https://example.test/WinTool-1.1.0.exe"), installer.Length);
        var progressValues = new List<UpdateDownloadProgress>();

        var filePath = await service.DownloadUpdateAsync(asset, new InlineProgress<UpdateDownloadProgress>(progressValues.Add));

        var expectedFilePath = fileSystem.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "WinTool",
            "WinTool-1.1.0.exe");

        Assert.Equal(expectedFilePath, filePath);
        Assert.Equal("https://example.test/WinTool-1.1.0.exe", handler.Request?.RequestUri?.AbsoluteUri);
        Assert.Equal(installer, fileSystem.File.ReadAllBytes(filePath));
        Assert.False(fileSystem.File.Exists(filePath + ".download"));
        Assert.Equal(100, progressValues[^1].Percentage);
    }

    [Fact]
    public async Task DownloadUpdateAsync_WhenCanceled_CancelsRequest()
    {
        var fileSystem = new MockFileSystem();
        var service = new UpdateService(new HttpClient(new CancellableHttpMessageHandler()), fileSystem, NullLogger<UpdateService>.Instance, new AppState());
        var asset = new GitHubReleaseAsset("WinTool-1.1.0.exe", new Uri("https://example.test/WinTool-1.1.0.exe"), 4);
        using var cancellationTokenSource = new CancellationTokenSource();

        var downloadTask = service.DownloadUpdateAsync(asset, cancellationToken: cancellationTokenSource.Token);
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => downloadTask);
    }

    private static UpdateService CreateService(HttpMessageHandler handler, Version? currentVersion = null, MockFileSystem? fileSystem = null)
    {
        var appState = currentVersion is null ? new AppState() : new AppState(currentVersion);
        return new UpdateService(new HttpClient(handler), fileSystem ?? new MockFileSystem(), NullLogger<UpdateService>.Instance, appState);
    }

    private static HttpResponseMessage CreateReleaseResponse(Version version, bool includeAsset)
    {
        var versionText = version.ToString(3);
        var tagName = $"v{versionText}";

        if (!includeAsset)
            return CreateJsonResponse($$"""{"tag_name":"{{tagName}}","html_url":"https://github.com/arcanexhoax/WinTool/releases/tag/{{tagName}}","assets":[]}""");

        return CreateJsonResponse($$"""{"tag_name":"{{tagName}}","html_url":"https://github.com/arcanexhoax/WinTool/releases/tag/{{tagName}}","assets":[{"name":"WinTool-{{versionText}}.exe","browser_download_url":"https://github.com/arcanexhoax/WinTool/releases/download/{{tagName}}/WinTool-{{versionText}}.exe","size":3,"id":42,"digest":"sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"}]}""");
    }

    private static HttpResponseMessage CreateJsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    private sealed class StubHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(response);
        }
    }

    private sealed class InlineProgress<T>(Action<T> callback) : IProgress<T>
    {
        public void Report(T value)
        {
            callback(value);
        }
    }

    private sealed class CancellableHttpMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
