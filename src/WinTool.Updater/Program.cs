using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace WinTool.Updater;

internal class Program
{
    private const int RebootRequiredExitCode = 3010;
    private const string ApplicationFileName = "WinTool.exe";
    private const string InstallParameter = "/install";
    private const string InstallerFileName = "Installer.exe";
    private const string PrepareParameter = "/prepare";
    private const string UpdaterFileName = "Updater.exe";

    private static readonly string s_dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WinTool");
    private static readonly string s_updateDirectory = Path.Combine(s_dataDirectory, "Updates");

    private static async Task<int> Main(string[] args)
    {
        try
        {
            return args.FirstOrDefault() switch
            {
                PrepareParameter => await PrepareUpdateAsync(args),
                InstallParameter => await InstallUpdateAsync(args),
                _ => 1,
            };
        }
        catch (Exception ex)
        {
            TryWriteLog(ex);
            return 1;
        }
    }

    private static async Task<int> PrepareUpdateAsync(string[] args)
    {
        if (args is not [PrepareParameter, var appProcessIdStr, var sourceInstallerPath, var assetIdStr, .. var appArgs]
            || !int.TryParse(appProcessIdStr, out int appProcessId)
            || !long.TryParse(assetIdStr, out long assetId)
            || assetId <= 0
            || !File.Exists(sourceInstallerPath)
            || Environment.ProcessPath is not string sourceUpdaterPath)
        {
            return 1;
        }

        var appPath = Path.Combine(AppContext.BaseDirectory, ApplicationFileName);

        if (!File.Exists(appPath))
            return 1;

        var asset = await GetReleaseAssetAsync(assetId);

        if (asset is null
            || asset.Id != assetId
            || asset.Size <= 0
            || !TryParseSha256Digest(asset.Digest, out var expectedInstallerHash))
        {
            return 1;
        }

        IOHelper.PrepareUpdateDirectory(s_dataDirectory, s_updateDirectory);

        var targetUpdaterPath = Path.Combine(s_updateDirectory, UpdaterFileName);
        var targetInstallerPath = Path.Combine(s_updateDirectory, InstallerFileName);

        File.Copy(sourceUpdaterPath, targetUpdaterPath, true);
        File.Copy(sourceInstallerPath, targetInstallerPath, true);

        if (!IOHelper.VerifyFile(targetInstallerPath, asset.Size, expectedInstallerHash))
            throw new InvalidDataException("The installer hash does not match the release asset digest.");

        File.Delete(sourceInstallerPath);

        var startInfo = new ProcessStartInfo(targetUpdaterPath)
        {
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add(InstallParameter);
        startInfo.ArgumentList.Add(appProcessId.ToString());
        startInfo.ArgumentList.Add(Environment.ProcessId.ToString());
        startInfo.ArgumentList.Add(targetInstallerPath);
        startInfo.ArgumentList.Add(appPath);

        foreach (string argument in appArgs)
            startInfo.ArgumentList.Add(argument);

        using var updater = Process.Start(startInfo);
        return updater is null ? 1 : 0;
    }

    private static async Task<int> InstallUpdateAsync(string[] args)
    {
        if (args is not [InstallParameter, var appProcessIdStr, var parentProcessIdStr, var installerPath, var appPath, .. var appArgs]
            || !int.TryParse(appProcessIdStr, out int appProcessId)
            || !int.TryParse(parentProcessIdStr, out int parentProcessId)
            || !IsUpdaterInUpdateDirectory()
            || !File.Exists(appPath))
        {
            return 1;
        }

        await ProcessHelper.WaitForProcessExitAsync(parentProcessId);
        await ProcessHelper.WaitForProcessExitAsync(appProcessId);

        using var installer = ProcessHelper.StartInstaller(installerPath, s_updateDirectory);
        await installer.WaitForExitAsync();

        if (installer.ExitCode is not (0 or RebootRequiredExitCode))
            return installer.ExitCode;

        File.Delete(installerPath);

        ProcessHelper.StartApplication(appPath, appArgs);
        return 0;
    }

    private static async Task<GitHubReleaseAsset?> GetReleaseAssetAsync(long assetId)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("WinTool-Updater", "1.0"));

        return await httpClient.GetFromJsonAsync<GitHubReleaseAsset>($"https://api.github.com/repos/arcanexhoax/WinTool/releases/assets/{assetId}");
    }

    private static bool TryParseSha256Digest(string? digest, out byte[] hash)
    {
        const string prefix = "sha256:";

        if (digest?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) != true)
        {
            hash = [];
            return false;
        }

        var value = digest[prefix.Length..];

        if (value.Length != SHA256.HashSizeInBytes * 2)
        {
            hash = [];
            return false;
        }

        try
        {
            hash = Convert.FromHexString(value);
            return true;
        }
        catch (FormatException)
        {
            hash = [];
            return false;
        }
    }

    private static bool IsUpdaterInUpdateDirectory()
    {
        var expectedPath = Path.GetFullPath(Path.Combine(s_updateDirectory, UpdaterFileName));
        var actualPath = Environment.ProcessPath is not null ? Path.GetFullPath(Environment.ProcessPath) : null;

        return string.Equals(actualPath, expectedPath, StringComparison.OrdinalIgnoreCase);
    }

    private static void TryWriteLog(Exception exception)
    {
        try
        {
            if (IsUpdaterInUpdateDirectory())
                File.AppendAllText(Path.Combine(s_updateDirectory, "update.log"), $"{DateTimeOffset.Now:O} {exception}\r\n");
        }
        catch
        {
        }
    }
}
