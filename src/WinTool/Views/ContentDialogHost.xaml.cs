using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WinTool.Views;

public partial class ContentDialogHost : Grid
{
    private readonly Storyboard _showStoryboard;
    private readonly Storyboard _closeStoryboard;

    public static ContentDialogHost? Current { get; private set; }

    public ContentDialogHost()
    {
        InitializeComponent();

        _showStoryboard = (Storyboard)Resources["OpenDialogStoryboard"];
        _closeStoryboard = (Storyboard)Resources["CloseDialogStoryboard"];

        Current = this;

        _closeStoryboard.Completed += OnCloseStoryboardCompleted;
    }

    public void Show(FrameworkElement content)
    {
        _closeStoryboard.Stop(this);

        DialogPresenter.Content = content;
        Visibility = Visibility.Visible;

        _showStoryboard.Begin(this, true);
    }

    public void Close()
    {
        if (Visibility != Visibility.Visible)
        {
            CompleteClose();
            return;
        }

        _showStoryboard.Stop(this);
        _closeStoryboard.Begin(this, true);
    }

    private void CompleteClose()
    {
        Visibility = Visibility.Collapsed;
        DialogPresenter.Content = null;
    }

    private void OnCloseStoryboardCompleted(object? sender, EventArgs e)
    {
        CompleteClose();
    }
}
