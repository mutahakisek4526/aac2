using System.Globalization;
using System.Windows.Data;

namespace AacV2.Converters;

public sealed class ProgressToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2)
        {
            return 0.0;
        }

        var progress = values[0] is double p ? p : 0.0;
        var width = values[1] is double w ? w : 0.0;
        return width * progress;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
