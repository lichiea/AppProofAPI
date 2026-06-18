using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AppProofAPI.Converters;

// Конвертер для цвета вердикта (PASSED/WARNING/FAILED)
public class VerdictToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "PASSED" => new SolidColorBrush(Color.Parse("#10b981")),   // Зеленый
            "WARNING" => new SolidColorBrush(Color.Parse("#f59e0b")), // Оранжевый
            "FAILED" => new SolidColorBrush(Color.Parse("#ef4444")),  // Красный
            _ => new SolidColorBrush(Color.Parse("#10b981"))          // По умолчанию зеленый
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

// Конвертер для цвета критичности уязвимости
public class SeverityToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Critical" => new SolidColorBrush(Color.Parse("#dc2626")), // Темно-красный
            "High" => new SolidColorBrush(Color.Parse("#ef4444")),     // Красный
            "Medium" => new SolidColorBrush(Color.Parse("#f59e0b")),   // Оранжевый
            "Low" => new SolidColorBrush(Color.Parse("#3b82f6")),      // Синий
            "Info" => new SolidColorBrush(Color.Parse("#6b7280")),     // Серый
            _ => new SolidColorBrush(Color.Parse("#3b82f6"))           // По умолчанию синий
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}