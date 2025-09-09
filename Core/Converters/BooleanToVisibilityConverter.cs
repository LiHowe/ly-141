using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Core.Converters;

/// <summary>
/// 布尔值到可见性的转换器
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            // 检查是否需要反转逻辑
            bool invert = parameter?.ToString()?.ToLower() == "invert";
            bool shouldShow = invert ? !boolValue : boolValue;
            
            return shouldShow ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            bool isVisible = visibility == Visibility.Visible;
            
            // 检查是否需要反转逻辑
            bool invert = parameter?.ToString()?.ToLower() == "invert";
            return invert ? !isVisible : isVisible;
        }
        return false;
    }
}