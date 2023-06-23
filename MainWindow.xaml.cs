using SHDocVw;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WinTool.Enum;
using WinTool.Model;
using WinTool.Modules;
using WinTool.Native;

namespace WinTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            KeyHooker hooker = new(Key.E);
            hooker.KeyHooked += OnKeyHooked;
        }

        private async void OnKeyHooked(object? sender, KeyHookedEventArgs e)
        {
            if (e.Modifier.HasFlag(KeyModifier.Ctrl))
            {
                if (e.Modifier.HasFlag(KeyModifier.Shift))
                {
                    string? path = await Shell.GetActiveExplorerPathAsync();

                    if (string.IsNullOrEmpty(path))
                        return;

                    DirectoryInfo di = new(path);
                    int num = 0;

                    try
                    {
                        var numbers = di.EnumerateFiles("NewFile_*.txt").Select(f => int.Parse(Regex.Match(f.Name, @"\d+").Value));

                        if (numbers.Any())
                            num = numbers.Max() + 1;

                        using (File.Create(Path.Combine(path, $"NewFile_{num}.txt"))) { }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
        }

        
    }
}
