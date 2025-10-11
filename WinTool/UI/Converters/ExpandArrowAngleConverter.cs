using System;
using System.Globalization;
using System.Windows.Data;

namespace WinTool.UI.Converters;

public class ExpandArrowAngleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is bool isExpanded && isExpanded) ? 180.0 : 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
