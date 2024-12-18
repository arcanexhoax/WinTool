using System.Windows;
using WinTool.Native;

namespace WinTool.View;

public class ModalWindow : Window
{
    public ModalWindow()
    {
        // TODO unsub
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        BringProcessToForeground();
    }

    private void BringProcessToForeground()
    {
        INPUT input = new()
        {
            Type = INPUTTYPE.INPUTMOUSE,
            Data = { }
        };
        INPUT[] inputs = [input];

        // Send empty mouse event. This makes this thread the last to send input, and hence allows it to pass foreground permission checks
        _ = NativeMethods.SendInput(1, inputs, INPUT.Size);
        Activate();
    }
}
