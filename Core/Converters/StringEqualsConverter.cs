using System.Globalization;
using System.Windows.Data;

namespace Core.Converters;

public class StringEqualsConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		// value 是 SelectedQuickOption，parameter 是当前 RadioButton 的 Tag
		if (value == null || parameter == null)
			return false;
		return value.ToString() == parameter.ToString();
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if ((bool)value)
			return parameter?.ToString();
		return Binding.DoNothing;
	}
}
