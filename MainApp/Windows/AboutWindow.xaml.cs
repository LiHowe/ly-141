using System.Diagnostics;
using System.Management;
using System.Reflection;
using System.Text;
using System.Windows;
using Core;
using Core.Models.Settings;
using Core.Utils;

namespace MainApp.Windows;

/// <summary>
///     AboutWindow.xaml 的交互逻辑
///     关于窗体 - 显示应用程序信息
/// </summary>
/// <summary>
///     AboutWindow.xaml 的交互逻辑
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        var systemSettings = ConfigManager.Instance.LoadConfig<SystemSettings>(Constants.SystemConfigFilePath);
        Title = $"关于 {systemSettings.SystemName}";
        AppVersionTextBlock.Text = systemSettings.SystemVersion;
        AppNameTextBlock.Text = systemSettings.SystemName;
        AppSubtitleTextBlock.Text = systemSettings.SystemSubName;
    }

    /// <summary>
    ///     系统信息按钮点击事件
    /// </summary>
    private void SystemInfoButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var systemInfo = GetSystemInformation();
            MessageBox.Show(systemInfo, "系统信息", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"获取系统信息失败: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    ///     关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    ///     获取系统信息
    /// </summary>
    private string GetSystemInformation()
    {
        var info = new StringBuilder();

        try
        {
            // 应用程序信息
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var location = assembly.Location;

            info.AppendLine("=== 应用程序信息 ===");
            info.AppendLine($"程序版本: {version}");
            info.AppendLine($"程序路径: {location}");
            info.AppendLine($"启动时间: {Process.GetCurrentProcess().StartTime:yyyy-MM-dd HH:mm:ss}");
            info.AppendLine();

            // 系统信息
            info.AppendLine("=== 系统信息 ===");
            info.AppendLine($"操作系统: {Environment.OSVersion}");
            info.AppendLine($"计算机名: {Environment.MachineName}");
            info.AppendLine($"用户名: {Environment.UserName}");
            info.AppendLine($"处理器数量: {Environment.ProcessorCount}");
            info.AppendLine($"系统目录: {Environment.SystemDirectory}");
            info.AppendLine();

            // 运行时信息
            info.AppendLine("=== 运行时信息 ===");
            info.AppendLine($".NET 版本: {Environment.Version}");
            info.AppendLine($"工作集内存: {Environment.WorkingSet / 1024 / 1024} MB");
            info.AppendLine($"GC 内存: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
            info.AppendLine($"运行时间: {DateTime.Now - Process.GetCurrentProcess().StartTime}");
            info.AppendLine();

            // 硬件信息（简化版）
            try
            {
                info.AppendLine("=== 硬件信息 ===");

                // CPU信息
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get()) info.AppendLine($"处理器: {obj["Name"]}");
                }

                // 内存信息
                using (var searcher =
                       new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var totalMemory = Convert.ToInt64(obj["TotalPhysicalMemory"]) / 1024 / 1024 / 1024;
                        info.AppendLine($"总内存: {totalMemory} GB");
                    }
                }
            }
            catch
            {
                info.AppendLine("硬件信息获取失败");
            }
        }
        catch (Exception ex)
        {
            info.AppendLine($"获取系统信息时发生错误: {ex.Message}");
        }

        return info.ToString();
    }
}