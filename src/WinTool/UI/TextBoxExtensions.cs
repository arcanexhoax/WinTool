using System.Windows;
using System.Windows.Controls;

namespace WinTool.UI;

public static class TextBoxExtensions
{
    public static readonly DependencyProperty IsTextSelectedProperty = 
        DependencyProperty.RegisterAttached("IsTextSelected", typeof(bool), typeof(TextBoxExtensions), new UIPropertyMetadata(false, OnIsTextSelectedPropertyChanged));

    public static bool GetIsTextSelected(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsTextSelectedProperty);
    }

    public static void SetIsTextSelected(DependencyObject obj, bool value)
    {
        obj.SetValue(IsTextSelectedProperty, value);
    }

    private static void OnIsTextSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var uie = (TextBox)d;

        if ((bool)e.NewValue)
        {
            uie.SelectAll();
        }
    }
}
