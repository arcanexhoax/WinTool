using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using WinTool.CommandLine;
using WinTool.Model;
using WinTool.Utils;
using WinTool.View;
using WinTool.ViewModel;
using Resource = WinTool.Resources.Localizations.Resources;

namespace WinTool.Modules
{
    public class CommandHandler
    {
        private static string s_lastRunWithArgsData = string.Empty;
        private static CreateFileData? s_lastCreateFileData;

        public static async Task CreateFileFast(string newFileTemplate)
        {
            string? path = await Shell.GetActiveExplorerPathAsync();

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

        public static async Task CreateFileInteractive()
        {
            string? path = await Shell.GetActiveExplorerPathAsync();

            if (string.IsNullOrEmpty(path))
                return;

            CreateFileViewModel createFileVm = new(path, s_lastCreateFileData, r =>
            {
                if (!r.Success || r.FilePath is null or [])
                    return;

                s_lastCreateFileData = r.CreateFileData;

                if (File.Exists(r.FilePath))
                {
                    MessageBox.Show(string.Format(Resource.FileAlreadyExists, r.FilePath), Resource.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string? driveLetter = Path.GetPathRoot(r.FilePath);

                if (string.IsNullOrEmpty(driveLetter))
                {
                    MessageBox.Show(string.Format(Resource.FilePathInvalid, r.FilePath), Resource.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var drive = DriveInfo.GetDrives().FirstOrDefault(d => string.Equals(d.Name, driveLetter, StringComparison.InvariantCultureIgnoreCase));

                if (drive is null)
                {
                    MessageBox.Show(string.Format(Resource.DriveNotFound, driveLetter), Resource.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (drive.AvailableFreeSpace < r.Size)
                {
                    MessageBox.Show(string.Format(Resource.OutOfMemory, driveLetter, drive.AvailableFreeSpace, r.Size), Resource.Error, 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CreateFile(r.FilePath, r.Size);
            });

            CreateFileView createFileView = new(createFileVm);
            createFileView.Show();
            createFileView.Activate();
        }

        public static void CreateFile(string path, long size = 0)
        {
            ProcessHelper.ExecuteWithUacIfNeeded(() =>
            {
                using var fileStream = File.Create(path);
                fileStream.SetLength(size);
            }, new CreateFileParameter() 
            {
                FilePath = path,
                Size = size
            });
        }

        public static async Task CopyFilePath()
        {
            var selectedPaths = await Shell.GetSelectedItemsPathsAsync();

            // if there are no selections - copy folder path
            if (selectedPaths.Count == 0)
            {
                string? folderPath = await Shell.GetActiveExplorerPathAsync();

                if (!string.IsNullOrEmpty(folderPath))
                    Clipboard.SetText(folderPath);
            }
            else
            {
                Clipboard.SetText(string.Join(Environment.NewLine, selectedPaths));
            }
        }

        public static async Task CopyFileName()
        {
            var selectedPaths = await Shell.GetSelectedItemsPathsAsync();

            // if there are no selections - copy folder name
            if (selectedPaths.Count == 0)
            {
                string? folderPath = await Shell.GetActiveExplorerPathAsync();

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

        public static async Task RunWithArgs()
        {
            var selectedPaths = await Shell.GetSelectedItemsPathsAsync();

            if (selectedPaths.Count != 1 || Path.GetExtension(selectedPaths[0]) != ".exe")
                return;

            string selectedItem = selectedPaths[0];
            RunWithArgsViewModel runWithArgsVm = new(selectedPaths[0], s_lastRunWithArgsData, r =>
            {
                if (r.Success)
                {
                    s_lastRunWithArgsData = r.Args ?? string.Empty;

                    if (File.Exists(selectedItem))
                        using (Process.Start(selectedItem, s_lastRunWithArgsData)) { }
                    else
                        MessageBox.Show(string.Format(Resource.FileNotFound, selectedItem), Resource.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            RunWithArgsWindow runWithArgsWindow = new(runWithArgsVm);
            runWithArgsWindow.Show();
            runWithArgsWindow.Activate();
        }

        public static async Task OpenInCmd()
        {
            string? folderPath = await Shell.GetActiveExplorerPathAsync();

            if (string.IsNullOrEmpty(folderPath))
                return;

            using (Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = folderPath,
                FileName = "cmd.exe",
                UseShellExecute = false,
            })) { }
        }
    }
}
