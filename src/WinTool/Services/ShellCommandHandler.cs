using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Extensions;
using WinTool.Models;
using WinTool.Properties;
using WinTool.Views.Shortcuts;

namespace WinTool.Services;

public class ShellCommandHandler(Shell shell, ViewFactory viewFactory, StaThreadService staThreadService)
{
    private const string NewFileTemplate = "NewFile.txt";

    private readonly Shell _shell = shell;
    private readonly ViewFactory _viewFactory = viewFactory;
    private readonly StaThreadService _staThreadService = staThreadService;

    public bool IsBackgroundMode { get; set; } = true;

    public void CreateFileFast()
    {
        string? path = _shell.GetActiveShellPath();

        if (string.IsNullOrEmpty(path))
            return;

        DirectoryInfo di = new(path);
        int num = 0;

        string? fileName = Path.GetFileNameWithoutExtension(NewFileTemplate);
        string? extension = Path.GetExtension(NewFileTemplate);

        // TODO test it after new file name template will match Windows behavior
        var numbers = di.EnumerateFiles($"{fileName}_*{extension}").Select(f =>
        {
            var match = Regex.Match(f.Name, $@"^{fileName}_(\d+){extension}$");

            if (match.Groups.Count != 2)
                return -1;

            if (int.TryParse(match.Groups[1].Value, out int number))
                return number;
            return -1;
        });

        if (numbers.Any())
            num = numbers.Max() + 1;

        CreateFile(Path.Combine(path, $"{fileName}_{num}{extension}"));
    }

    public void CreateFileInteractive()
    {
        string? path = _shell.GetActiveShellPath();

        if (string.IsNullOrEmpty(path))
            return;

        var result = _viewFactory.ShowDialog<CreateFileWindow, string, CreateFileOutput>(path);

        if (result is not { Success: true, Data: { } data })
            return;

        CreateFile(data.FilePath, data.Size);
    }

    public void CreateFile(string path, long size = 0)
    {
        var clp = new CommandLineParameters()
        {
            BackgroundParameter = IsBackgroundMode ? new BackgroundParameter() : null,
            CreateFileParameter = new CreateFileParameter()
            {
                FilePath = path,
                Size = size
            }
        };

        Process.ExecuteAsAdmin(() =>
        {
            using var fileStream = File.Create(path);
            fileStream.SetLength(size);
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
            RunFileAsAdminOptional(selectedItem, null, true);
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
            MessageBox.ShowError(string.Format(Resources.FileNotFound, selectedItem));
            return;
        }

        RunFileAsAdminOptional(selectedItem, data.Args, data.RunAsAdmin);
    }

    private void RunFileAsAdminOptional(string fileName, string? args, bool asAdmin)
    {
        try
        {
            Process.Start(fileName, args, asAdmin);
        }
        catch (Win32Exception ex) when (ex.HResult == -2147467259)
        {
            Debug.WriteLine($"File {fileName} is unable to run as admin");
            Process.Start(fileName, args, false);
        }
    }

    public void OpenInCmd() => OpenInCmd(false);

    public void OpenInCmdAsAdmin() => OpenInCmd(true);

    private void OpenInCmd(bool asAdmin)
    {
        string? folderPath = _shell.GetActiveShellPath();

        if (!string.IsNullOrEmpty(folderPath))
            Process.Start("cmd.exe", $"/k cd /d \"{folderPath}\"", asAdmin);
    }
}
