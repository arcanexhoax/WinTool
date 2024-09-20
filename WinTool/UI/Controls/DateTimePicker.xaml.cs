using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WinTool.UI.Controls
{
    public partial class DateTimePicker : UserControl
    {
        private const string DateTimeFormat = "dd.MM.yyyy HH:mm";

        public DateTime SelectedDate
        {
            get => (DateTime)GetValue(SelectedDateProperty);
            set => SetValue(SelectedDateProperty, value);
        }

        public static readonly DependencyProperty SelectedDateProperty = 
            DependencyProperty.Register(nameof(SelectedDate),
                                        typeof(DateTime), 
                                        typeof(DateTimePicker), 
                                        new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

        public DateTimePicker()
        {
            InitializeComponent();

            HoursDisplay.ItemsSource = Enumerable.Range(0, 24).Select(n => n.ToString("D2"));
            MinutesDisplay.ItemsSource = Enumerable.Range(0, 60).Select(n => n.ToString("D2"));
        }

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DateTimePicker picker || e.NewValue is not DateTime date)
                return;

            picker.CalendarDisplay.SelectedDate = date;
            picker.HoursDisplay.SelectedItem = date.Hour.ToString("D2");
            picker.MinutesDisplay.SelectedItem = date.Minute.ToString("D2");
        }

        private void SaveTime_Click(object sender, RoutedEventArgs e)
        {
            var time = TimeSpan.Parse($"{HoursDisplay.SelectedItem}:{MinutesDisplay.SelectedItem}");
            var date = CalendarDisplay.SelectedDate!.Value + time;

            SelectedDate = date;
            PopUpCalendarButton.IsChecked = false;
        }
    }
}
