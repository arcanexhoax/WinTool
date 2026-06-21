using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Extensions;
using WinTool.Models;
using WinTool.Native;
using WinTool.Views.Shortcuts;

namespace WinTool.Services;

public class ShellCommandHandler(ILogger<ShellCommandHandler> logger, Shell shell, ViewFactory viewFactory, StaThreadService staThreadService)
{
    // File Explorer and Notepad resources used to compose the localized "New Text Document" name.
    // The resource IDs are undocumented, so keep fallbacks.
    private const string NewItemNameTemplateResource = "@shell32.dll,-30316";
    private const string TextDocumentNameResource = "@notepad.exe,-469";
    private const string NewItemNameTemplateFallback = "New %s";
    private const string TextDocumentNameFallback = "Text Document";

    private readonly ILogger _logger = logger;
    private readonly Shell _shell = shell;
    private readonly ViewFactory _viewFactory = viewFactory;
    private readonly StaThreadService _staThreadService = staThreadService;

    public bool IsBackgroundMode { get; set; } = true;

    public void CreateFile()
    {
        var path = _shell.GetActiveShellPath();

        if (string.IsNullOrEmpty(path))
            return;

        var availablePath = GetAvailableFilePath(path, GetNewFileName());
        CreateFile(availablePath);
    }

    public void CreateFile(string path)
    {
        var clp = new CommandLineParameters()
        {
            BackgroundParameter = IsBackgroundMode ? new BackgroundParameter() : null,
            CreateFileParameter = new CreateFileParameter()
            {
                FilePath = path,
            }
        };

        Process.ExecuteAsAdmin(() =>
        {
            File.Open(path, FileMode.CreateNew).Dispose();
            _shell.BeginRename(path);

            _logger.LogInformation("Created file '{FilePath}'.", path);
        }, clp.ToString());
    }

    public void CopyFilePath()
    {
        var selectedItems = _shell.GetSelectedItems();

        // if there are no selections - copy folder path
        if (selectedItems.Count == 0)
        {
            string? folderPath = _shell.GetActiveShellPath();

            if (!string.IsNullOrEmpty(folderPath))
                _staThreadService.Invoke(() => Clipboard.SetText(folderPath));
        }
        else
        {
            var filePaths = selectedItems.Select(i => i.Path);
            _staThreadService.Invoke(() => Clipboard.SetText(string.Join(Environment.NewLine, filePaths)));
        }
    }

    public void CopyFileName()
    {
        var selectedItems = _shell.GetSelectedItems();

        // if there are no selections - copy folder name
        if (selectedItems.Count == 0)
        {
            string? folderPath = _shell.GetActiveShellPath();

            if (string.IsNullOrEmpty(folderPath))
                return;

            DirectoryInfo di = new(folderPath);
            _staThreadService.Invoke(() => Clipboard.SetText(di.Name));
        }
        else
        {
            var fileNames = selectedItems.Select(i => i.Name ?? i.Path);
            _staThreadService.Invoke(() => Clipboard.SetText(string.Join(Environment.NewLine, fileNames)));
        }
    }

    public void RunFileAsAdmin()
    {
        var selectedItems = _shell.GetSelectedItems();

        if (selectedItems is [{ Path: var selectedItem }])
            Process.Start(selectedItem, null, true);
    }

    public void RunFileWithArgs()
    {
        var selectedItems = _shell.GetSelectedItems();

        if (selectedItems is not [{ Path: var selectedItem }])
            return;

        var result = _viewFactory.ShowDialog<RunWithArgsWindow, string, RunWithArgsOutput>(selectedItem);

        if (result is not { Success: true, Data: { } data })
            return;

        if (!File.Exists(selectedItem))
        {
            _logger.LogWarning("File {FilePath} does not exist", selectedItem);
            return;
        }

        Process.Start(selectedItem, data.Args, data.RunAsAdmin);
    }

    public void OpenInCmd() => OpenInCmd(false);

    public void OpenInCmdAsAdmin() => OpenInCmd(true);

    private void OpenInCmd(bool asAdmin)
    {
        string? folderPath = _shell.GetActiveShellPath();

        if (!string.IsNullOrEmpty(folderPath))
            Process.Start("cmd.exe", $"/k cd /d \"{folderPath}\"", asAdmin);
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
