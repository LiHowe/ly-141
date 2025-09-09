using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core.Models;

namespace Core.Converters
{
    /// <summary>
    /// 图标转换器，用于将菜单图标配置转换为对应的UI元素
    /// </summary>
    public class IconConverter : IMultiValueConverter
    {
        /// <summary>
        /// 将图标配置转换为UI元素
        /// </summary>
        /// <param name="values">values[0]: Icon字符串, values[1]: IconType枚举</param>
        /// <param name="targetType">目标类型</param>
        /// <param name="parameter">参数</param>
        /// <param name="culture">文化信息</param>
        /// <returns>TextBlock或Image控件</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return DependencyProperty.UnsetValue;

            var icon = values[0] as string;
            if (string.IsNullOrEmpty(icon))
                return DependencyProperty.UnsetValue;

            if (values[1] is not IconType iconType)
                iconType = IconType.Text;

            try
            {
                switch (iconType)
                {
                    case IconType.Text:
                        return CreateTextIcon(icon);
                    case IconType.Image:
                        return CreateImageIcon(icon);
                    default:
                        return CreateTextIcon(icon);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"图标转换失败: {ex.Message}");
                // 转换失败时返回默认文本图标
                return CreateTextIcon("📄");
            }
        }

        /// <summary>
        /// 创建文本图标
        /// </summary>
        private TextBlock CreateTextIcon(string iconText)
        {
            return new TextBlock
            {
                Text = iconText,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            };
        }

        /// <summary>
        /// 创建图片图标
        /// </summary>
        private Image CreateImageIcon(string iconPath)
        {
            var image = new Image
            {
                Width = 16,
                Height = 16,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0),
                Stretch = Stretch.Uniform
            };

            // 尝试加载图片
            try
            {
                BitmapImage bitmap;

                // 检查是否为绝对路径
                if (Path.IsPathRooted(iconPath))
                {
                    if (File.Exists(iconPath))
                    {
                        bitmap = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
                    }
                    else
                    {
                        throw new FileNotFoundException($"图标文件不存在: {iconPath}");
                    }
                }
                else
                {
                    // 相对路径，尝试多种方式加载
                    bitmap = LoadRelativeImage(iconPath);
                }

                image.Source = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载图标失败: {iconPath}, 错误: {ex.Message}");
                // 加载失败时显示默认图标
                image.Source = CreateDefaultIcon();
            }

            return image;
        }

        /// <summary>
        /// 加载相对路径图片
        /// </summary>
        private BitmapImage LoadRelativeImage(string relativePath)
        {
            // 尝试不同的路径组合
            var possiblePaths = new[]
            {
                $"pack://application:,,,/{relativePath}",
                $"pack://application:,,,/MainApp;component/{relativePath}",
                $"pack://application:,,,/Resources/{relativePath}",
                $"pack://application:,,,/MainApp;component/Resources/{relativePath}",
                $"pack://application:,,,/Resources/Images/{relativePath}",
                $"pack://application:,,,/MainApp;component/Resources/Images/{relativePath}"
            };

            foreach (var path in possiblePaths)
            {
                try
                {
                    var bitmap = new BitmapImage(new Uri(path, UriKind.Absolute));
                    // 如果能成功创建，返回该bitmap
                    return bitmap;
                }
                catch
                {
                    // 继续尝试下一个路径
                    continue;
                }
            }

            // 如果所有路径都失败，尝试从应用程序目录加载
            var appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
            if (File.Exists(appPath))
            {
                return new BitmapImage(new Uri(appPath, UriKind.Absolute));
            }

            throw new FileNotFoundException($"无法找到图标文件: {relativePath}");
        }

        /// <summary>
        /// 创建默认图标
        /// </summary>
        private BitmapSource CreateDefaultIcon()
        {
            // 创建一个简单的默认图标（16x16像素的灰色方块）
            var width = 16;
            var height = 16;
            var dpi = 96;
            var pixelFormat = PixelFormats.Bgr32;
            var stride = (width * pixelFormat.BitsPerPixel + 7) / 8;
            var pixels = new byte[height * stride];

            // 填充灰色
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 128;     // Blue
                pixels[i + 1] = 128; // Green
                pixels[i + 2] = 128; // Red
                pixels[i + 3] = 255; // Alpha
            }

            return BitmapSource.Create(width, height, dpi, dpi, pixelFormat, null, pixels, stride);
        }

        /// <summary>
        /// 反向转换（不支持）
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("IconConverter不支持反向转换");
        }
    }
}
