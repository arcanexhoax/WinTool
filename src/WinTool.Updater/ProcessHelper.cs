using System.Diagnostics;

namespace WinTool.Updater;

internal class ProcessHelper
{
    public static Process StartInstaller(string installerPath, string updateDirectory)
    {
        var startInfo = new ProcessStartInfo(installerPath)
        {
            Arguments = $@"/passive /norestart /log ""{Path.Combine(updateDirectory, "update.log")}""",
            UseShellExecute = false,
        };

        return Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start the installer.");
    }

    // TODO support start with elevated privileges (runas) if needed
    public static void StartApplication(string applicationPath, string[] applicationArguments)
    {
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
