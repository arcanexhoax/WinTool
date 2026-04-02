using System.Windows;
using System.Windows.Media;
using PrimitiveToggleButton = System.Windows.Controls.Primitives.ToggleButton;

namespace WinTool.UI.Controls;

public enum ToggleSwitchStateContentPosition
{
    Left,
    Right,
}

public class ToggleSwitch : PrimitiveToggleButton
{
    private const double KnobCheckedTranslateX = 10;

    private static readonly DependencyPropertyKey CurrentContentPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(CurrentContent),
        typeof(object),
        typeof(ToggleSwitch),
        new PropertyMetadata(WinTool.Properties.Resources.Off));

    public static readonly DependencyProperty CurrentContentProperty = CurrentContentPropertyKey.DependencyProperty;

    public static readonly DependencyProperty ShowStateContentProperty = DependencyProperty.Register(
        nameof(ShowStateContent),
        typeof(bool),
        typeof(ToggleSwitch),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty StateContentPositionProperty = DependencyProperty.Register(
        nameof(StateContentPosition),
        typeof(ToggleSwitchStateContentPosition),
        typeof(ToggleSwitch),
        new FrameworkPropertyMetadata(ToggleSwitchStateContentPosition.Right, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty OnContentProperty = DependencyProperty.Register(
        nameof(OnContent),
        typeof(object),
        typeof(ToggleSwitch),
        new FrameworkPropertyMetadata(WinTool.Properties.Resources.On, FrameworkPropertyMetadataOptions.AffectsMeasure, OnStateContentPropertyChanged));

    public static readonly DependencyProperty OffContentProperty = DependencyProperty.Register(
        nameof(OffContent),
        typeof(object),
        typeof(ToggleSwitch),
        new FrameworkPropertyMetadata(WinTool.Properties.Resources.Off, FrameworkPropertyMetadataOptions.AffectsMeasure, OnStateContentPropertyChanged));

    static ToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
    }

    public ToggleSwitch()
    {
        Checked += OnCheckedChanged;
        Unchecked += OnCheckedChanged;
        Indeterminate += OnCheckedChanged;

        UpdateCurrentContent();
    }

    public object CurrentContent
    {
        get => GetValue(CurrentContentProperty);
        private set => SetValue(CurrentContentPropertyKey, value);
    }

    public bool ShowStateContent
    {
        get => (bool)GetValue(ShowStateContentProperty);
        set => SetValue(ShowStateContentProperty, value);
    }

    public ToggleSwitchStateContentPosition StateContentPosition
    {
        get => (ToggleSwitchStateContentPosition)GetValue(StateContentPositionProperty);
        set => SetValue(StateContentPositionProperty, value);
    }

    public object OnContent
    {
        get => GetValue(OnContentProperty);
        set => SetValue(OnContentProperty, value);
    }

    public object OffContent
    {
        get => GetValue(OffContentProperty);
        set => SetValue(OffContentProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        UpdateKnobPosition();
        UpdateCurrentContent();
    }

    private static void OnStateContentPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        ((ToggleSwitch)dependencyObject).UpdateCurrentContent();
    }

    private void OnCheckedChanged(object sender, RoutedEventArgs e)
    {
        UpdateCurrentContent();
    }

    private void UpdateKnobPosition()
    {
        if (GetTemplateChild("KnobTranslate") is TranslateTransform knobTranslate)
            knobTranslate.X = IsChecked == true ? KnobCheckedTranslateX : -KnobCheckedTranslateX;
    }

    private void UpdateCurrentContent()
    {
        CurrentContent = IsChecked == true ? ResolveOnContent() : ResolveOffContent();
    }

    private object ResolveOnContent()
    {
        return OnContent ?? WinTool.Properties.Resources.On;
    }

    private object ResolveOffContent()
    {
        return OffContent ?? WinTool.Properties.Resources.Off;
    }
}
