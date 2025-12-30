using System;
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
using File = System.IO.File;

namespace WinTool.Services;

public class ShellCommandHandler(Shell shell, ViewFactory viewFactory)
{
    private const string NewFileTemplate = "NewFile.txt";

    private readonly Shell _shell = shell;
    private readonly ViewFactory _viewFactory = viewFactory;

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

        var createFileWindow = _viewFactory.Create<CreateFileWindow>();
        var result = createFileWindow.ShowDialog(path);

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
        }, clp);
    }

    public void CopyFilePath()
    {
        var selectedItems = _shell.GetSelectedItems();

        // if there are no selections - copy folder path
        if (selectedItems.Count == 0)
        {
            string? folderPath = _shell.GetActiveShellPath();

            if (!string.IsNullOrEmpty(folderPath))
                Clipboard.SetText(folderPath);
        }
        else
        {
            var filePaths = selectedItems.Select(i => i.Path);
            Clipboard.SetText(string.Join(Environment.NewLine, filePaths));
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
            Clipboard.SetText(di.Name);
        }
        else
        {
            var fileNames = selectedItems.Select(i => i.Name ?? i.Path);
            Clipboard.SetText(string.Join(Environment.NewLine, fileNames));
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

        var runWithArgsWindow = _viewFactory.Create<RunWithArgsWindow>();
        var result = runWithArgsWindow.ShowDialog(selectedItem);

        if (result is not { Success: true, Data: { } data })
            return;

        if (!File.Exists(selectedItem))
        {
            MessageBox.ShowError(string.Format(Resources.FileNotFound, selectedItem));
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

    public void ChangeFileProperties()
    {
        var selectedItems = _shell.GetSelectedItems();

        if (selectedItems.Count == 0)
            return;

        var selectedItemPath = selectedItems[0].Path;

        if (!File.Exists(selectedItemPath))
            return;

        TagLib.File? tfile = null;

        try
        {
            tfile = TagLib.File.Create(selectedItemPath);
        }
        catch (Exception ex) when (ex.Source == "taglib-sharp")
        { 
        }
        catch 
        {
            throw;
        }

        var changeFilePropertiesView = _viewFactory.Create<ChangeFilePropertiesWindow>();
        var result = changeFilePropertiesView.ShowDialog(new ChangeFilePropertiesInput(
            selectedItemPath,
            tfile != null,
            tfile?.Tag.Title,
            tfile?.Tag.Performers,
            tfile?.Tag.Album,
            tfile?.Tag.Genres,
            tfile?.Tag.Lyrics,
            tfile?.Tag.Year ?? 0,
            File.GetCreationTime(selectedItemPath),
            File.GetLastAccessTime(selectedItemPath)));

        if (result is not { Success: true, Data: { } data })
        {
            if (result.Message is not (null or []))
                MessageBox.ShowError(result.Message);
            return;
        }
            
        if (tfile != null)
        {
            tfile!.Tag.Title = data.Title;
            tfile.Tag.Performers = data.Performers;
            tfile.Tag.Album = data.Album;
            tfile.Tag.Genres = data.Genres;
            tfile.Tag.Lyrics = data.Lyrics;
            tfile.Tag.Year = data.Year;
            tfile.Save();
            tfile.Dispose();
        }

        File.SetCreationTime(selectedItemPath, data.CreationTime);
        File.SetLastWriteTime(selectedItemPath, data.ChangeTime);
    }
}
