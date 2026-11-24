using CommunityToolkit.WinUI;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using WinTool.WinUI.CommandLine;
using WinTool.WinUI.Models;
using WinTool.WinUI.Native;

namespace WinTool.WinUI.Services;

public class ShellCommandHandler(ILogger<ShellCommandHandler> logger, Shell shell, ProcessHelper processHelper, AppState appState, DialogService dialogService)
{
    // File Explorer and Notepad resources used to compose the localized "New Text Document" name.
    // The resource IDs are undocumented, so keep fallbacks.
    private const string NewItemNameTemplateResource = "@shell32.dll,-30316";
    private const string TextDocumentNameResource = "@notepad.exe,-469";
    private const string NewItemNameTemplateFallback = "New %s";
    private const string TextDocumentNameFallback = "Text Document";

    private readonly ILogger _logger = logger;
    private readonly Shell _shell = shell;
    private readonly ProcessHelper _processHelper = processHelper;
    private readonly AppState _appState = appState;
    private readonly DialogService _dialogService = dialogService;

    public Task CreateFileAsync()
    {
        var path = _shell.GetActiveShellPath();

        if (string.IsNullOrEmpty(path))
            return Task.CompletedTask;

        var availablePath = GetAvailableFilePath(path, GetNewFileName());
        CreateFile(availablePath);
        return Task.CompletedTask;
    }

    public void CreateFile(string path)
    {
        var clp = new CommandLineParameters()
        {
            BackgroundParameter = _appState.IsBackgroundMode ? new BackgroundParameter() : null,
            CreateFileParameter = new CreateFileParameter()
            {
                FilePath = path,
            }
        };

        _processHelper.ExecuteAsAdmin(() =>
        {
            File.Open(path, FileMode.CreateNew).Dispose();
            NativeMethods.NotifyShellFileCreated(path);
            _shell.BeginRename(path);

            _logger.LogInformation("Created file '{FilePath}'.", path);
        }, clp.ToString());
    }

    public Task CopyFilePathAsync()
    {
        var selectedItems = _shell.GetSelectedItems();

        // if there are no selections - copy folder path
        if (selectedItems.Count == 0)
        {
            string? folderPath = _shell.GetActiveShellPath();

            if (!string.IsNullOrEmpty(folderPath))
                SetClipboardTextAsync(folderPath);
        }
        else
        {
            var filePaths = selectedItems.Select(i => i.Path);
            SetClipboardTextAsync(string.Join(Environment.NewLine, filePaths));
        }

        return Task.CompletedTask;
    }

    public Task CopyFileNameAsync()
    {
        var selectedItems = _shell.GetSelectedItems();

        // if there are no selections - copy folder name
        if (selectedItems.Count == 0)
        {
            string? folderPath = _shell.GetActiveShellPath();

            if (string.IsNullOrEmpty(folderPath))
                return Task.CompletedTask;

            DirectoryInfo di = new(folderPath);
            SetClipboardTextAsync(di.Name);
        }
        else
        {
            var fileNames = selectedItems.Select(i => i.Name ?? i.Path);
            SetClipboardTextAsync(string.Join(Environment.NewLine, fileNames));
        }

        return Task.CompletedTask;
    }

    public Task RunFileAsAdminAsync()
    {
        var selectedItems = _shell.GetSelectedItems();

        if (selectedItems is [{ Path: var selectedItem }])
            _processHelper.Start(selectedItem, null, true);

        return Task.CompletedTask;
    }

    public async Task RunFileWithArgsAsync()
    {
        var selectedItems = _shell.GetSelectedItems();

        if (selectedItems is not [{ Path: var selectedItem }])
            return;

        var result = await _dialogService.ShowRunWithArgsAsync(selectedItem);

        if (result is not { Success: true, Data: { } data })
            return;

        if (!File.Exists(selectedItem))
        {
            _logger.LogWarning("File {FilePath} does not exist", selectedItem);
            return;
        }

        if (data.KeepConsoleOpen)
            _processHelper.StartInCmd(selectedItem, data.Args, data.RunAsAdmin);
        else
            _processHelper.Start(selectedItem, data.Args, data.RunAsAdmin);
    }

    public Task OpenInCmdAsync() => OpenInCmd(false);

    public Task OpenInCmdAsAdminAsync() => OpenInCmd(true);

    private Task OpenInCmd(bool asAdmin)
    {
        string? folderPath = _shell.GetActiveShellPath();

        if (!string.IsNullOrEmpty(folderPath))
            _processHelper.Start("cmd.exe", $"/k cd /d \"{folderPath}\"", asAdmin);

        return Task.CompletedTask;
    }

    private Task SetClipboardTextAsync(string text)
    {
        return App.Dispatcher.EnqueueAsync(() =>
        {
            var data = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
            data.SetText(text);

            Clipboard.SetContent(data);
            Clipboard.Flush();
        });
    }

    internal static string GetAvailableFilePath(string directoryPath, string fileName)
    {
        string name = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        string path = Path.Combine(directoryPath, fileName);

        if (!Path.Exists(path))
            return path;

        for (int number = 2; ; number++)
        {
            path = Path.Combine(directoryPath, $"{name} ({number}){extension}");

            if (!Path.Exists(path))
                return path;
        }
    }

    internal static string GetNewFileName()
    {
        string template = NativeMethods.LoadIndirectString(NewItemNameTemplateResource) ?? NewItemNameTemplateFallback;
        string documentName = NativeMethods.LoadIndirectString(TextDocumentNameResource) ?? TextDocumentNameFallback;

        return $"{template.Replace("%s", documentName)}.txt";
    }
}
