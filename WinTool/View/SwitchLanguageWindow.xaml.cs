using System;
using System.Windows;
using System.Windows.Interop;
using WinTool.Native;

namespace WinTool.View
{
    public partial class SwitchLanguageWindow : Window
    {
        public SwitchLanguageWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.ShowWindowAsPopup(hwnd);
        }
    }
}
