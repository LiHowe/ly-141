using System.Globalization;
using System.Windows.Data;

namespace Core.Converters;
// MultiBinding converter：values[0] = SelectedQuickOption，values[1] = 当前项
public class SelectedEqualsItemConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values == null || values.Length < 2)
			return false;

		var selected = values[0]?.ToString();
		var item = values[1]?.ToString();
		return string.Equals(selected, item, StringComparison.Ordinal);
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException(); // Mode=OneWay，因此不会调用
	}
}