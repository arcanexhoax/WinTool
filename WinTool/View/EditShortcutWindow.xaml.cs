using System;
using System.Windows;
using System.Windows.Interop;
using WinTool.Model;
using WinTool.ViewModel;

namespace WinTool.View;

public partial class EditShortcutWindow : Window
{
    public EditShortcutWindow(EditShortcutViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }

    public Result<string> ShowDialog(string shortcut)
    {
        var vm = (EditShortcutViewModel)DataContext;
        Result<string>? result = null;

        SourceInitialized += (_, __) =>
        {
            var handle = new WindowInteropHelper(this).Handle;

            vm.StartEditing(handle, shortcut, r =>
            {
                result = r;
                Close();
            });
        };

        ShowDialog();
        vm.StopEditing();

        return result ?? new Result<string>(false);
    }
}
