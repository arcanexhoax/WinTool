﻿using GlobalKeyInterceptor;
using GlobalKeyInterceptor.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using WindowsInput;
using WinTool.CommandLine;
using WinTool.Utils;
using WinTool.View;
using WinTool.ViewModel;
using Resource = WinTool.Resources.Localizations.Resources;

namespace WinTool.Modules
{
    public class CommandHandler
    {
        private static readonly IKeyboardSimulator _keyboardSimulator;

        static CommandHandler()
        {
            _keyboardSimulator = new InputSimulator().Keyboard;
        }

        public static async Task CreateFileFast(string newFileTemplate)
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

                CreateFile(Path.Combine(path, $"{fileName}_{num}{extension}"));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public static async Task CreateFileInteractive()
        {
            string? path = await Shell.GetActiveExplorerPathAsync();

            if (string.IsNullOrEmpty(path))
                return;

            CreateFileViewModel createFileVm = new(path, r =>
            {
                if (!r.Success || string.IsNullOrEmpty(r.FilePath))
                    return;

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
            createFileView.ShowFocused();
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
                        MessageBox.Show(string.Format(Resource.FileNotFound, selectedItem), Resource.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            RunWithArgsWindow runWithArgsWindow = new(runWithArgsVm);
            runWithArgsWindow.ShowFocused();
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

        public static Task UpperCaseSelectedText()
        {
            ChangeSelectedTextCase(true); 
            return Task.CompletedTask;
        }

        public static Task LowerCaseSelectedText()
        {
            ChangeSelectedTextCase(false);
            return Task.CompletedTask;
        }

        private static void ChangeSelectedTextCase(bool toUpper)
        {
            string backingText = string.Empty;
            bool leftCtrlPressed = KeyUtils.IsKeyPressed(Key.LeftCtrl);
            bool rightCtrlPressed = KeyUtils.IsKeyPressed(Key.RightCtrl);
            bool leftShiftPressed = KeyUtils.IsKeyPressed(Key.LeftShift);
            bool rightShiftPressed = KeyUtils.IsKeyPressed(Key.RightShift);
            bool leftAltPressed = KeyUtils.IsKeyPressed(Key.LeftAlt);
            bool rightAltPressed = KeyUtils.IsKeyPressed(Key.RightAlt);
            bool leftWinPressed = KeyUtils.IsKeyPressed(Key.LeftWindows);
            bool rightWinPressed = KeyUtils.IsKeyPressed(Key.RightWindows);

            try
            {
                backingText = Clipboard.GetText();
                Clipboard.Clear();

                // release all of the modifier buttons to send Ctrl+C (V) correct 
                _keyboardSimulator.KeyUp(VirtualKeyCode.LSHIFT,
                                         VirtualKeyCode.RSHIFT,
                                         VirtualKeyCode.LMENU,
                                         VirtualKeyCode.RMENU,
                                         VirtualKeyCode.LWIN,
                                         VirtualKeyCode.RWIN);

                System.Windows.Forms.SendKeys.SendWait("^c");

                string copiedText = Clipboard.GetText();

                if (string.IsNullOrEmpty(copiedText))
                    return;

                copiedText = toUpper ? copiedText.ToUpper() : copiedText.ToLower();

                Clipboard.Clear();
                Clipboard.SetText(copiedText);
                System.Windows.Forms.SendKeys.SendWait("^v");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Change text case failed: {ex.Message}");
            }
            finally
            {
                // in some cases SetText throws exception "CLIPBRD_E_CANT_OPEN"
                try
                {
                    if (!string.IsNullOrEmpty(backingText))
                        Clipboard.SetText(backingText);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Set backing copied text failed: {ex.Message}");
                }

                // restore all pressed modifiers before command was invoked
                if (leftCtrlPressed)
                    _keyboardSimulator.KeyDown(VirtualKeyCode.LCONTROL);
                else if (rightCtrlPressed)
                    _keyboardSimulator.KeyDown(VirtualKeyCode.RCONTROL);

                if (leftShiftPressed)
                    _keyboardSimulator.KeyDown(VirtualKeyCode.LSHIFT);
                else if (rightShiftPressed)
                    _keyboardSimulator.KeyDown(VirtualKeyCode.RSHIFT);

                if (leftAltPressed)
                    _keyboardSimulator.KeyDown(VirtualKeyCode.LMENU);
                else if (rightAltPressed)
                    _keyboardSimulator.KeyDown(VirtualKeyCode.RMENU);

                if (leftWinPressed)
                    _keyboardSimulator.KeyDown(VirtualKeyCode.LWIN);
                else if (rightWinPressed)
                    _keyboardSimulator.KeyDown(VirtualKeyCode.RWIN);
            }
        }

        public static async Task SearchText()
        {
            string? folderPath = await Shell.GetActiveExplorerPathAsync();

            if (string.IsNullOrEmpty(folderPath))
                return;

            SearchTextViewModel vm = new(folderPath);
            SearchTextView view = new(vm);
            view.ShowFocused();
        }
    }
}
