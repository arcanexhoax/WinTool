using System.Diagnostics;

namespace WinTool.Updater;

internal class ProcessHelper
{
    public static Process StartInstaller(string installerPath, string updateDirectory, string applicationDirectory)
    {
        var startInfo = new ProcessStartInfo(installerPath)
        {
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add("/passive");
        startInfo.ArgumentList.Add("/norestart");
        startInfo.ArgumentList.Add("/log");
        startInfo.ArgumentList.Add(Path.Combine(updateDirectory, "update.log"));
        startInfo.ArgumentList.Add($"InstallFolder={applicationDirectory}");

        return Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start the installer.");
    }

    public static void StartApplication(string applicationPath, string[] applicationArguments, bool isElevated)
    {
        if (isElevated)
        {
            var startInfo = new ProcessStartInfo(applicationPath)
            {
                UseShellExecute = false,
            };

            foreach (string argument in applicationArguments)
                startInfo.ArgumentList.Add(argument);

            Process.Start(startInfo);
            return;
        }

        var shellType = Type.GetTypeFromProgID("Shell.Application") ?? throw new InvalidOperationException("Unable to access the Windows shell.");
        dynamic shell = Activator.CreateInstance(shellType) ?? throw new InvalidOperationException("Unable to create the Windows shell.");
        shell.ShellExecute(applicationPath, string.Join(' ', applicationArguments), Path.GetDirectoryName(applicationPath), null, 1);
    }

    public static async Task WaitForProcessExitAsync(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            await process.WaitForExitAsync();
        }
        catch (ArgumentException)
        {
        }
    }
}
