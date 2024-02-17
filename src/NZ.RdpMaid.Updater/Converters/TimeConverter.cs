using System.Globalization;
using System.Windows.Data;

namespace NZ.RdpMaid.Updater.Converters;

internal class TimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not null && value is TimeSpan time)
        {
            return $"[{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}]";
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}