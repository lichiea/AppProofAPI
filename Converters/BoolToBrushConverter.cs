using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AppProofAPI.Converters;

public class BoolToWarningBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return new SolidColorBrush(Color.Parse("#fef3c7")); // жёлтый фон
        return new SolidColorBrush(Color.Parse("#f0f9ff")); // обычный светлый
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}