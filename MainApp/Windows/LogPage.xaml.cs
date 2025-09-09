using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Core.Models;
using MainApp.ViewModels;

namespace MainApp.Windows;

/// <summary>
///     LogPage.xaml 的交互逻辑
/// </summary>
public partial class LogPage : UserControl, IDisposable
{
    private bool _disposed;
    private LogPageViewModel? _viewModel;

    /// <summary>
    ///     构造函数
    /// </summary>
    public LogPage()
    {
        InitializeComponent();
        // 创建并设置ViewModel
        _viewModel ??= new LogPageViewModel();
        DataContext = _viewModel;
    }

    public LogPage(LogPageViewModel vm) : this()
    {
        _viewModel = vm;
        DataContext = _viewModel;
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _viewModel?.Dispose();
            _viewModel = null;
            _disposed = true;
        }
    }
}

/// <summary>
///     日志等级到字符串转换器
/// </summary>
public class LogLevelToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is LogLevel level)
            return level switch
            {
                LogLevel.Debug => "调试",
                LogLevel.Info => "信息",
                LogLevel.Warning => "警告",
                LogLevel.Error => "错误",
                _ => value.ToString()
            };
        return "全部";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///     布尔值反转转换器
/// </summary>
public class BooleanInverseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        return false;
    }
}

/// <summary>
///     布尔值到是否转换器
/// </summary>
public class BooleanToYesNoConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue ? "是" : "否";
        return "否";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///     空值到可见性转换器
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///     空值到布尔值转换器
/// </summary>
public class NullToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}