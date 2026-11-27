using GlobalKeyInterceptor;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinTool.WinUI.ViewModels.Shortcuts;

namespace WinTool.WinUI.Views.Shortcuts;

public partial class ShortcutView : UserControl
{
    public ShortcutViewModel? ViewModel => DataContext as ShortcutViewModel;

    public ShortcutView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) => Bindings.Update();

    private Visibility GetShortcutVisibility(Shortcut? shortcut) => shortcut is not null ? Visibility.Visible : Visibility.Collapsed;

    private Visibility GetNotSpecifiedVisibility(Shortcut? shortcut) => shortcut is null ? Visibility.Visible : Visibility.Collapsed;

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e) => EditButton.Visibility = Visibility.Visible;

    private void OnPointerExited(object sender, PointerRoutedEventArgs e) => EditButton.Visibility = Visibility.Collapsed;
}
