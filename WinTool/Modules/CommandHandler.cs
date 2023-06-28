using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using WinTool.View;
using WinTool.ViewModel;

namespace WinTool.Modules
{
    public class CommandHandler
    {
        public static async Task FastCreateFile(string newFileTemplate)
        {
            string? path = await Shell.GetActiveExplorerPathAsync();

            if (string.IsNullOrEmpty(path))
                return;

            DirectoryInfo di = new(path);
            int num = 0;
            string? fileName = Path.GetFileNameWithoutExtension(newFileTemplate);
            string? extension = Path.GetExtension(newFileTemplate);

            try
            {
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

                using (File.Create(Path.Combine(path, $"{fileName}_{num}{extension}"))) { }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public static async Task CreateFile()
        {
            string? path = await Shell.GetActiveExplorerPathAsync();

            if (string.IsNullOrEmpty(path))
                return;

            CreateFileViewModel createFileVm = new(path, r =>
            {
                if (r.Success && !string.IsNullOrEmpty(r.FilePath))
                {
                    if (!File.Exists(r.FilePath))
                        using (File.Create(r.FilePath)) { }
                    else
                        MessageBox.Show($"File '{r.FilePath}' already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            CreateFileView createFileView = new(createFileVm);
            createFileView.Show();
            createFileView.Activate();
        }

        public static async Task CopyFilePath()
        {
            var selectedPaths = await Shell.GetSelectedItemsPathsAsync();

            // if there are no selections - copy folder path, if one item selected - copy item's path, else - not copying
            if (selectedPaths.Count == 0)
            {
                string? folderPath = await Shell.GetActiveExplorerPathAsync();

                if (!string.IsNullOrEmpty(folderPath))
                    Clipboard.SetText(folderPath);
            }
            else if (selectedPaths.Count == 1)
            {
                Clipboard.SetText(selectedPaths[0]);
            }
        }

        public static async Task CopyFileName()
        {
            var selectedPaths = await Shell.GetSelectedItemsPathsAsync();

            // if there are no selections - copy folder name, if one item selected - copy item's name, else - not copying
            if (selectedPaths.Count == 0)
            {
                string? folderPath = await Shell.GetActiveExplorerPathAsync();

                if (string.IsNullOrEmpty(folderPath))
                    return;

                DirectoryInfo di = new(folderPath);
                Clipboard.SetText(di.Name);
            }
            else if (selectedPaths.Count == 1)
            {
                string fileName = Path.GetFileName(selectedPaths[0]);
                Clipboard.SetText(fileName);
            }
        }

        public static async Task RunWithArgs()
        {
            var selectedPaths = await Shell.GetSelectedItemsPathsAsync();

            if (selectedPaths.Count != 1 || Path.GetExtension(selectedPaths[0]) != ".exe")
                return;

            string selectedItem = selectedPaths[0];
            RunWithArgsViewModel runWithArgsVm = new(selectedPaths[0], r =>
            {
                if (r.Success)
                {
                    if (File.Exists(selectedItem))
                        using (Process.Start(selectedItem, r.Args ?? string.Empty)) { }
                    else
                        MessageBox.Show($"File '{selectedItem}' doesn't exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
