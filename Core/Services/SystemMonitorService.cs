using System.Diagnostics;
using System.IO;
using System.Management;
using Logger;


namespace Core.Services;

/// <summary>
/// 系统监控服务
/// 提供系统性能监控、资源使用情况监控等功能
/// </summary>
public class SystemMonitorService : IDisposable
{
    private readonly Timer _monitorTimer;
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memoryCounter;
    private readonly Process _currentProcess;
    private bool _disposed = false;

    public SystemMonitorService()
    {
        _currentProcess = Process.GetCurrentProcess();

        // 初始化性能计数器
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");

        // 初始化监控定时器
        _monitorTimer = new Timer(MonitorCallback, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

        Log.Debug("系统监控服务已启动");
    }

    

    /// <summary>
    /// 性能监控事件
    /// </summary>
    public event Action<SystemPerformance>? PerformanceUpdated;

    /// <summary>
    /// 获取当前系统性能
    /// </summary>
    public SystemPerformance GetCurrentPerformance()
    {
        try
        {
            var performance = new SystemPerformance
            {
                Timestamp = DateTime.Now,
                Uptime = DateTime.Now - _currentProcess.StartTime
            };

            // CPU使用率
            performance.CpuUsage = _cpuCounter.NextValue();

            // 内存信息
            performance.AvailableMemoryMB = (long)_memoryCounter.NextValue();
            performance.TotalMemoryMB = GetTotalPhysicalMemoryMB();
            performance.MemoryUsagePercent = ((double)(performance.TotalMemoryMB - performance.AvailableMemoryMB) /
                                              performance.TotalMemoryMB) * 100;

            // 进程信息
            _currentProcess.Refresh();
            performance.ProcessMemoryMB = _currentProcess.WorkingSet64 / 1024 / 1024;
            performance.ThreadCount = _currentProcess.Threads.Count;
            performance.HandleCount = _currentProcess.HandleCount;

            return performance;
        }
        catch (Exception ex)
        {
            Log.Error("获取系统性能信息失败", ex);
            return new SystemPerformance { Timestamp = DateTime.Now };
        }
    }

    /// <summary>
    /// 获取磁盘信息
    /// </summary>
    public List<DiskInfo> GetDiskInfo()
    {
        var diskInfos = new List<DiskInfo>();

        try
        {
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (drive.IsReady)
                {
                    var totalSizeGB = drive.TotalSize / 1024 / 1024 / 1024;
                    var availableSizeGB = drive.AvailableFreeSpace / 1024 / 1024 / 1024;
                    var usedSizeGB = totalSizeGB - availableSizeGB;

                    diskInfos.Add(new DiskInfo
                    {
                        DriveName = drive.Name,
                        DriveType = drive.DriveType.ToString(),
                        TotalSizeGB = totalSizeGB,
                        AvailableSizeGB = availableSizeGB,
                        UsagePercent = totalSizeGB > 0 ? (double)usedSizeGB / totalSizeGB * 100 : 0
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("获取磁盘信息失败", ex);
        }

        return diskInfos;
    }

    /// <summary>
    /// 获取网络信息
    /// </summary>
    public List<NetworkInfo> GetNetworkInfo()
    {
        var networkInfos = new List<NetworkInfo>();

        try
        {
            using var searcher =
                new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetEnabled=true");
            foreach (ManagementObject obj in searcher.Get())
            {
                var interfaceName = obj["Name"]?.ToString() ?? "Unknown";
                var status = obj["NetConnectionStatus"]?.ToString() ?? "Unknown";

                networkInfos.Add(new NetworkInfo
                {
                    InterfaceName = interfaceName,
                    Status = GetNetworkStatus(status),
                    BytesSent = 0, // 需要额外的性能计数器来获取
                    BytesReceived = 0,
                    SendRate = 0,
                    ReceiveRate = 0
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error("获取网络信息失败", ex);
        }

        return networkInfos;
    }

    /// <summary>
    /// 获取系统信息摘要
    /// </summary>
    public Dictionary<string, object> GetSystemSummary()
    {
        var summary = new Dictionary<string, object>();

        try
        {
            var performance = GetCurrentPerformance();
            var diskInfos = GetDiskInfo();

            summary["SystemTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            summary["Uptime"] = performance.Uptime.ToString(@"dd\.hh\:mm\:ss");
            summary["CpuUsage"] = $"{performance.CpuUsage:F1}%";
            summary["MemoryUsage"] = $"{performance.MemoryUsagePercent:F1}%";
            summary["ProcessMemory"] = $"{performance.ProcessMemoryMB} MB";
            summary["ThreadCount"] = performance.ThreadCount;
            summary["HandleCount"] = performance.HandleCount;

            // 磁盘使用情况
            var systemDisk = diskInfos.FirstOrDefault(d => d.DriveName.StartsWith("C:"));
            if (systemDisk != null)
            {
                summary["DiskUsage"] = $"{systemDisk.UsagePercent:F1}%";
                summary["DiskFree"] = $"{systemDisk.AvailableSizeGB} GB";
            }

            // 操作系统信息
            summary["OSVersion"] = Environment.OSVersion.ToString();
            summary["MachineName"] = Environment.MachineName;
            summary["UserName"] = Environment.UserName;
            summary["ProcessorCount"] = Environment.ProcessorCount;
        }
        catch (Exception ex)
        {
            Log.Error("获取系统摘要失败", ex);
        }

        return summary;
    }

    /// <summary>
    /// 检查系统健康状态
    /// </summary>
    public SystemHealthStatus CheckSystemHealth()
    {
        var status = new SystemHealthStatus();

        try
        {
            var performance = GetCurrentPerformance();
            var diskInfos = GetDiskInfo();

            // CPU健康检查
            if (performance.CpuUsage > 90)
                status.AddWarning("CPU使用率过高");
            else if (performance.CpuUsage > 80)
                status.AddInfo("CPU使用率较高");

            // 内存健康检查
            if (performance.MemoryUsagePercent > 90)
                status.AddWarning("内存使用率过高");
            else if (performance.MemoryUsagePercent > 80)
                status.AddInfo("内存使用率较高");

            // 磁盘健康检查
            foreach (var disk in diskInfos)
            {
                if (disk.UsagePercent > 95)
                    status.AddWarning($"磁盘 {disk.DriveName} 空间不足");
                else if (disk.UsagePercent > 85)
                    status.AddInfo($"磁盘 {disk.DriveName} 空间较少");
            }

            // 进程健康检查
            if (performance.ProcessMemoryMB > 1000)
                status.AddInfo("应用程序内存使用较高");

            if (performance.ThreadCount > 100)
                status.AddInfo("应用程序线程数较多");

            status.OverallStatus = status.Warnings.Count > 0 ? "Warning" :
                status.Infos.Count > 0 ? "Info" : "Healthy";
        }
        catch (Exception ex)
        {
            Log.Error("检查系统健康状态失败", ex);
            status.AddWarning("系统健康检查失败");
        }

        return status;
    }

    /// <summary>
    /// 监控回调
    /// </summary>
    private void MonitorCallback(object? state)
    {
        try
        {
            var performance = GetCurrentPerformance();
            PerformanceUpdated?.Invoke(performance);

            // 记录性能日志（每分钟记录一次）
            if (DateTime.Now.Second == 0)
            {
                Log.Info(
                    $"系统性能: CPU={performance.CpuUsage:F1}%, 内存={performance.MemoryUsagePercent:F1}%, 进程内存={performance.ProcessMemoryMB}MB");
            }
        }
        catch (Exception ex)
        {
            Log.Error("系统监控回调失败", ex);
        }
    }

    /// <summary>
    /// 获取总物理内存
    /// </summary>
    private long GetTotalPhysicalMemoryMB()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                return Convert.ToInt64(obj["TotalPhysicalMemory"]) / 1024 / 1024;
            }
        }
        catch
        {
            // 如果WMI查询失败，返回一个估算值
        }

        return 8192; // 默认8GB
    }

    /// <summary>
    /// 获取网络状态描述
    /// </summary>
    private string GetNetworkStatus(string status)
    {
        return status switch
        {
            "2" => "Connected",
            "7" => "Disconnected",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _monitorTimer?.Dispose();
            _cpuCounter?.Dispose();
            _memoryCounter?.Dispose();
            _currentProcess?.Dispose();
            _disposed = true;
            Log.Info("系统监控服务已停止");
        }
    }
}

/// <summary>
/// 系统健康状态
/// </summary>
public class SystemHealthStatus
{
    public string OverallStatus { get; set; } = "Unknown";
    public List<string> Warnings { get; set; } = new();
    public List<string> Infos { get; set; } = new();
    public DateTime CheckTime { get; set; } = DateTime.Now;

    public void AddWarning(string message)
    {
        Warnings.Add(message);
    }

    public void AddInfo(string message)
    {
        Infos.Add(message);
    }
}

/// <summary>
/// 系统性能信息
/// </summary>
public class SystemPerformance
{
    public double CpuUsage { get; set; }
    public long AvailableMemoryMB { get; set; }
    public long TotalMemoryMB { get; set; }
    public double MemoryUsagePercent { get; set; }
    public long ProcessMemoryMB { get; set; }
    public double ProcessCpuUsage { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public TimeSpan Uptime { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 磁盘信息
/// </summary>
public class DiskInfo
{
    public string DriveName { get; set; } = string.Empty;
    public string DriveType { get; set; } = string.Empty;
    public long TotalSizeGB { get; set; }
    public long AvailableSizeGB { get; set; }
    public double UsagePercent { get; set; }
}

/// <summary>
/// 网络信息
/// </summary>
public class NetworkInfo
{
    public string InterfaceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
    public double SendRate { get; set; }
    public double ReceiveRate { get; set; }
}