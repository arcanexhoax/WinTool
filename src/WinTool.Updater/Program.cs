using System.Diagnostics;

namespace WinTool.Updater;

internal class Program
{
    private const int RebootRequiredExitCode = 3010;

    private static async Task<int> Main(string[] args)
    {
        if (args is not [var proccessIdStr, var installerPath, var applicationPath, .. var applicationArguments]
            || !int.TryParse(proccessIdStr, out int processId)
            || !File.Exists(installerPath)
            || !File.Exists(applicationPath))
        {
            return 1;
        }

        try
        {
            await WaitForProcessExitAsync(processId);

            using var installer = StartInstaller(installerPath);
            await installer.WaitForExitAsync();

            if (installer.ExitCode is not (0 or RebootRequiredExitCode))
                return installer.ExitCode;

            var startInfo = new ProcessStartInfo(applicationPath)
            {
                UseShellExecute = true,
            };

            foreach (string argument in applicationArguments)
                startInfo.ArgumentList.Add(argument);

            Process.Start(startInfo);
            return 0;
        }
        catch
        {
            return 1;
        }
    }

    private static Process StartInstaller(string installerPath)
    {
        var startInfo = new ProcessStartInfo("msiexec.exe")
        {
            Arguments = $@"/i ""{installerPath}"" /passive /norestart /L*v ""{Path.Combine(AppContext.BaseDirectory, "update.log")}""",
            UseShellExecute = false,
        };

        return Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start Windows Installer.");
    }

    private static async Task WaitForProcessExitAsync(int processId)
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
