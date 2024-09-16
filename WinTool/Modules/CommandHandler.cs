using Microsoft.Extensions.Caching.Memory;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Utils;
using WinTool.View;
using WinTool.ViewModel;

namespace WinTool.Modules
{
    public class CommandHandler(Shell shell, MemoryCache memoryCache)
    {
        private readonly Shell _shell = shell;
        private readonly MemoryCache _memoryCache = memoryCache;

        public bool IsBackgroundMode { get; set; } = true;

        public async Task CreateFileFast(string newFileTemplate)
        {
            string? path = await _shell.GetActiveExplorerPathAsync();

            if (string.IsNullOrEmpty(path))
                return;

            DirectoryInfo di = new(path);
            int num = 0;
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

        public async Task CreateFileInteractive()
        {
            string? path = await _shell.GetActiveExplorerPathAsync();

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
                try
                {
                    using var fileStream = File.Create(path);
                    fileStream.SetLength(size);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to create file: " + ex.Message);
                }
            }, clp);
        }

        public async Task CopyFilePath()
        {
            var selectedPaths = await _shell.GetSelectedItemsPathsAsync();

            // if there are no selections - copy folder path
            if (selectedPaths.Count == 0)
            {
                string? folderPath = await _shell.GetActiveExplorerPathAsync();

                if (!string.IsNullOrEmpty(folderPath))
                    Clipboard.SetText(folderPath);
            }
            else
            {
                Clipboard.SetText(string.Join(Environment.NewLine, selectedPaths));
            }
        }

        public async Task CopyFileName()
        {
            var selectedPaths = await _shell.GetSelectedItemsPathsAsync();

            // if there are no selections - copy folder name
            if (selectedPaths.Count == 0)
            {
                string? folderPath = await _shell.GetActiveExplorerPathAsync();

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

        public async Task RunWithArgs()
        {
            var selectedPaths = await _shell.GetSelectedItemsPathsAsync();

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

        public async Task OpenInCmd()
        {
            string? folderPath = await _shell.GetActiveExplorerPathAsync();

            if (string.IsNullOrEmpty(folderPath))
                return;

            using (Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = folderPath,
                FileName = "cmd.exe",
                UseShellExecute = false,
            })) { }
        }

        public async Task ChangeFileProperties()
        {
            var selectedPaths = await _shell.GetSelectedItemsPathsAsync();

            if (selectedPaths.Count == 0)
                return;

            var selectedItemPath = selectedPaths[0];

            if (!File.Exists(selectedItemPath))
                return;

            var tfile = TagLib.File.Create(selectedItemPath);

            var changeFilePropertiesVm = new ChangeFilePropertiesViewModel(selectedItemPath, tfile);
            var changeFilePropertiesView = new ChangeFilePropertiesView(changeFilePropertiesVm);

            changeFilePropertiesView.Show();
            changeFilePropertiesView.Activate();
        }
    }
}
