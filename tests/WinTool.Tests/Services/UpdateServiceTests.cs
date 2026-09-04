using System.Net;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using WinTool.Models;
using WinTool.Services;

namespace WinTool.Tests.Services;

public class UpdateServiceTests
{
    [Theory]
    [InlineData("v1.2.4", "1.2.3", true)]
    [InlineData("1.2.3", "1.2.3", false)]
    [InlineData("v1.2.2", "1.2.3", false)]
    public async Task CheckForUpdateAsync_ComparesReleaseVersion(
        string tagName,
        string currentVersion,
        bool expectedUpdateAvailable)
    {
        var normalizedTag = tagName.TrimStart('v');
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal("https://api.github.com/repos/arcanexhoax/WinTool/releases/latest", request.RequestUri?.ToString());
            Assert.Contains(request.Headers.Accept, value => value.MediaType == "application/vnd.github+json");
            Assert.Equal($"WinTool/{currentVersion}", request.Headers.UserAgent.ToString());

            return CreateJsonResponse($$"""{"tag_name":"{{tagName}}","html_url":"https://github.com/arcanexhoax/WinTool/releases/tag/{{tagName}}","assets":[{"name":"WinTool-{{normalizedTag}}.msi","browser_download_url":"https://github.com/arcanexhoax/WinTool/releases/download/{{tagName}}/WinTool-{{normalizedTag}}.msi","size":3}]}""");
        });
        var service = CreateService(handler);

        var result = await service.CheckForUpdateAsync(Version.Parse(currentVersion));

        Assert.Equal(expectedUpdateAvailable, result.IsUpdateAvailable);
        Assert.Equal(Version.Parse(normalizedTag), result.LatestVersion);
        Assert.Equal($"https://github.com/arcanexhoax/WinTool/releases/tag/{tagName}", result.ReleaseUri.AbsoluteUri);

        if (expectedUpdateAvailable)
            Assert.Equal($"WinTool-{normalizedTag}.msi", result.Asset?.Name);
        else
            Assert.Null(result.Asset);
    }

    [Fact]
    public async Task CheckForUpdateAsync_WithInvalidTag_ThrowsInvalidDataException()
    {
        var handler = new StubHttpMessageHandler(_ => CreateJsonResponse("""{"tag_name":"latest","html_url":"https://github.com/arcanexhoax/WinTool/releases/latest"}"""));
        var service = CreateService(handler);

        await Assert.ThrowsAsync<InvalidDataException>(() => service.CheckForUpdateAsync(new Version(1, 0, 0)));
    }

    [Fact]
    public async Task CheckForUpdateAsync_WithoutInstallerAsset_ThrowsInvalidDataException()
    {
        var handler = new StubHttpMessageHandler(_ => CreateJsonResponse("""{"tag_name":"v1.1.0","html_url":"https://github.com/arcanexhoax/WinTool/releases/tag/v1.1.0","assets":[]}"""));
        var service = CreateService(handler);

        await Assert.ThrowsAsync<InvalidDataException>(() => service.CheckForUpdateAsync(new Version(1, 0, 0)));
    }

    [Fact]
    public async Task DownloadUpdateAsync_DownloadsInstallerToWinToolDataDirectory()
    {
        byte[] installer = [1, 2, 3, 4];
        var handler = new StubHttpMessageHandler(request =>
        {
            Assert.Equal("https://example.test/WinTool-1.1.0.msi", request.RequestUri?.AbsoluteUri);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(installer),
            };
        });
        var fileSystem = new MockFileSystem();
        var service = CreateService(handler, fileSystem);
        var asset = new GitHubReleaseAsset("WinTool-1.1.0.msi", new Uri("https://example.test/WinTool-1.1.0.msi"), installer.Length);
        var progressValues = new List<UpdateDownloadProgress>();

        var filePath = await service.DownloadUpdateAsync(asset, new InlineProgress<UpdateDownloadProgress>(progressValues.Add));

        var expectedFilePath = fileSystem.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "WinTool",
            "WinTool-1.1.0.msi");

        Assert.Equal(expectedFilePath, filePath);
        Assert.Equal(installer, fileSystem.File.ReadAllBytes(filePath));
        Assert.False(fileSystem.File.Exists(filePath + ".download"));
        Assert.Equal(100, progressValues[^1].Percentage);
    }

    [Fact]
    public async Task DownloadUpdateAsync_WhenCanceled_CancelsRequest()
    {
        var fileSystem = new MockFileSystem();
        var service = new UpdateService(new HttpClient(new CancellableHttpMessageHandler()), fileSystem);
        var asset = new GitHubReleaseAsset("WinTool-1.1.0.msi", new Uri("https://example.test/WinTool-1.1.0.msi"), 4);
        using var cancellationTokenSource = new CancellationTokenSource();

        var downloadTask = service.DownloadUpdateAsync(asset, cancellationToken: cancellationTokenSource.Token);
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => downloadTask);
    }

    private static UpdateService CreateService(HttpMessageHandler handler, MockFileSystem? fileSystem = null)
    {
        return new UpdateService(new HttpClient(handler), fileSystem ?? new MockFileSystem());
    }

    private static HttpResponseMessage CreateJsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(handler(request));
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
