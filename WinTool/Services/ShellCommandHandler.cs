using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Models;
using WinTool.Properties;
using WinTool.Utils;
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
        string? path = _shell.GetActiveExplorerPath();

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
        string? path = _shell.GetActiveExplorerPath();

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

        ProcessHelper.ExecuteWithUacIfNeeded(() =>
        {
            using var fileStream = File.Create(path);
            fileStream.SetLength(size);
        }, clp);
    }

    public void CopyFilePath()
    {
        var selectedPaths = _shell.GetSelectedItemsPaths();

        // if there are no selections - copy folder path
        if (selectedPaths.Count == 0)
        {
            string? folderPath = _shell.GetActiveExplorerPath();

            if (!string.IsNullOrEmpty(folderPath))
                Clipboard.SetText(folderPath);
        }
        else
        {
            Clipboard.SetText(string.Join(Environment.NewLine, selectedPaths));
        }
    }

    public void CopyFileName()
    {
        var selectedPaths = _shell.GetSelectedItemsPaths();

        // if there are no selections - copy folder name
        if (selectedPaths.Count == 0)
        {
            string? folderPath = _shell.GetActiveExplorerPath();

            if (string.IsNullOrEmpty(folderPath))
                return;

            DirectoryInfo di = new(folderPath);
            Clipboard.SetText(di.Name);
        }
        else
        {
            var fileNames = selectedPaths.Select(p => Path.GetFileName(p));
            Clipboard.SetText(string.Join(Environment.NewLine, fileNames));
        }
    }

    public void RunWithArgs()
    {
        var selectedPaths = _shell.GetSelectedItemsPaths();

        if (selectedPaths.Count != 1 || Path.GetExtension(selectedPaths[0]) != ".exe")
            return;

        string selectedItem = selectedPaths[0];

        var runWithArgsWindow = _viewFactory.Create<RunWithArgsWindow>();
        var result = runWithArgsWindow.ShowDialog(selectedItem);

        if (result is not { Success: true, Data: { } args })
            return;

        if (!File.Exists(selectedItem))
        {
            MessageBoxHelper.ShowError(string.Format(Resources.FileNotFound, selectedItem));
            return;
        }

        Process.Start(selectedItem, args ?? string.Empty);
    }

    public void OpenInCmd()
    {
        string? folderPath = _shell.GetActiveExplorerPath();

        if (string.IsNullOrEmpty(folderPath))
            return;

        using (Process.Start(new ProcessStartInfo
        {
            WorkingDirectory = folderPath,
            FileName = "cmd.exe",
            UseShellExecute = false,
        })) { }
    }

    public void ChangeFileProperties()
    {
        var selectedPaths = _shell.GetSelectedItemsPaths();

        if (selectedPaths.Count == 0)
            return;

        var selectedItemPath = selectedPaths[0];

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
                MessageBoxHelper.ShowError(result.Message);
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
