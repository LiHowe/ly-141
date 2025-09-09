using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace Core.Converters;

/// <summary>
/// 枚举描述转换器，将枚举值转换为其Description属性
/// </summary>
public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;
        var type = value.GetType();
        if (!type.IsEnum) return value.ToString();

        var field = type.GetField(value.ToString());
        var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
        return attribute?.Description ?? value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
