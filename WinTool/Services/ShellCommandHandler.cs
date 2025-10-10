using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Model;
using WinTool.Options;
using WinTool.Utils;
using WinTool.View;
using WinTool.ViewModel;
using File = System.IO.File;

namespace WinTool.Services;

public class ShellCommandHandler(Shell shell, MemoryCache memoryCache, IOptionsMonitor<ShortcutsOptions> shortcutsOptions, ChangeFilePropertiesViewModel changeFilePropertiesViewModel)
{
    private readonly Shell _shell = shell;
    private readonly MemoryCache _memoryCache = memoryCache;
    private readonly IOptionsMonitor<ShortcutsOptions> _shortcutsOptions = shortcutsOptions;
    private readonly ChangeFilePropertiesViewModel _changeFilePropertiesViewModel = changeFilePropertiesViewModel;

    public bool IsBackgroundMode { get; set; } = true;

    public void CreateFileFast()
    {
        string? path = _shell.GetActiveExplorerPath();

        if (string.IsNullOrEmpty(path))
            return;

        DirectoryInfo di = new(path);
        int num = 0;

        string newFileTemplate = _shortcutsOptions.CurrentValue.FastFileCreation.NewFileTemplate;
        string? fileName = Path.GetFileNameWithoutExtension(newFileTemplate);
        string? extension = Path.GetExtension(newFileTemplate);

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

        CreateFileViewModel createFileVm = new(path, _memoryCache, r =>
        {
            if (!r.Success || r.FilePath is null or [])
                return;

            CreateFile(r.FilePath, r.Size);
        });

        CreateFileView createFileView = new(createFileVm);
        createFileView.Show();
        createFileView.Activate();
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

        RunWithArgsViewModel runWithArgsVm = new(selectedItem, _memoryCache, r =>
        {
            if (r.Success)
                using (Process.Start(selectedItem, r.Args ?? string.Empty)) { }
        });

        RunWithArgsWindow runWithArgsWindow = new(runWithArgsVm);
        runWithArgsWindow.Show();
        runWithArgsWindow.Activate();
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

        var changeFilePropertiesView = new ChangeFilePropertiesView(_changeFilePropertiesViewModel);
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

        if (!result.Success || result.Data is not { } data)
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
