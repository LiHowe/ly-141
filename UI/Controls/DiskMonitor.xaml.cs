using System.IO;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Core;
using Core.Alarm;

using Core.Models.Settings;
using Core.Services;
using Core.Utils;
using Logger;
using Timer = System.Timers.Timer;

namespace UI.Controls;

public partial class DiskMonitor : UserControl
{
    private readonly DiskMonitorViewModel _viewModel;

    public DiskMonitor()
    {
        InitializeComponent();
        _viewModel = new DiskMonitorViewModel();
        DataContext = _viewModel;
    }

    public DiskMonitor(DiskMonitorViewModel vm) : this()
    {
        _viewModel = vm ?? new DiskMonitorViewModel();
        DataContext = _viewModel;
    }

    /// <summary>
    ///     外部只需要指定磁盘盘符，其余由控件内部控制
    /// </summary>
    public void SetDisk(string disk)
    {
        _viewModel?.SetDisk(disk);
    }

    /// <summary>
    ///     启动监控
    /// </summary>
    public void StartMonitoring()
    {
        _viewModel?.StartMonitoring();
    }

    /// <summary>
    ///     停止监控
    /// </summary>
    public void StopMonitoring()
    {
        _viewModel?.StopMonitoring();
    }
}

/// <summary>
///     磁盘监控视图模型, 用来监视磁盘余量，达到系统设定的阈值后进行报警(AlarmCoordinator)通知
/// </summary>
public partial class DiskMonitorViewModel : ObservableObject, IDisposable
{
    #region 构造函数

    public DiskMonitorViewModel()
    {
        LoadSystemSettings();

        // 初始化定时器，每5秒检查一次磁盘状态
        _monitorTimer = new Timer(5000);
        _monitorTimer.Elapsed += OnMonitorTimerElapsed;
        _monitorTimer.AutoReset = true;

        // 初始化磁盘信息
        UpdateDiskInfo();

        Log.Debug("磁盘监控视图模型已初始化");
    }

    #endregion

    #region 私有字段

    private readonly Timer _monitorTimer;
    private SystemSettings _systemSettings;
    private bool _disposed;
    private bool _isAlarmTriggered; // 防止重复报警

    #endregion

    #region 可观察属性

    /// <summary>
    ///     磁盘盘符 (如: "C:", "D:")
    /// </summary>
    [ObservableProperty] private string _disk = "C:";

    /// <summary>
    ///     磁盘名称显示文本
    /// </summary>
    [ObservableProperty] private string _diskName = "C:";

    /// <summary>
    ///     磁盘使用情况显示文本
    /// </summary>
    [ObservableProperty] private string _diskUsageText = "0%";

    /// <summary>
    ///     磁盘总容量 (GB)
    /// </summary>
    [ObservableProperty] private long _totalSizeGB;

    /// <summary>
    ///     磁盘可用空间 (GB)
    /// </summary>
    [ObservableProperty] private long _availableSizeGB;

    /// <summary>
    ///     磁盘使用率百分比
    /// </summary>
    [ObservableProperty] private double _usagePercent;

    /// <summary>
    ///     状态颜色画刷
    /// </summary>
    [ObservableProperty] private Brush _statusBrush = new SolidColorBrush(Colors.Green);

    /// <summary>
    ///     是否正在监控
    /// </summary>
    [ObservableProperty] private bool _isMonitoring;

    #endregion

    #region 公共方法

    /// <summary>
    ///     设置要监控的磁盘
    /// </summary>
    /// <param name="disk">磁盘盘符 (如: "C:", "D:")</param>
    public void SetDisk(string disk)
    {
        if (string.IsNullOrWhiteSpace(disk))
            return;

        // 确保磁盘格式正确
        if (!disk.EndsWith(":"))
            disk += ":";

        Disk = disk.ToUpper();
        DiskName = Disk;

        // 立即更新磁盘信息
        UpdateDiskInfo();

        Log.Debug($"设置监控磁盘: {Disk}");
    }

    /// <summary>
    ///     启动磁盘监控
    /// </summary>
    public void StartMonitoring()
    {
        if (IsMonitoring)
            return;

        _monitorTimer.Start();
        IsMonitoring = true;

        Log.Info($"开始监控磁盘 {Disk}");
    }

    /// <summary>
    ///     停止磁盘监控
    /// </summary>
    public void StopMonitoring()
    {
        if (!IsMonitoring)
            return;

        _monitorTimer.Stop();
        IsMonitoring = false;

        Log.Info($"停止监控磁盘 {Disk}");
    }

    #endregion

    #region 私有方法

    /// <summary>
    ///     加载系统设置
    /// </summary>
    private void LoadSystemSettings()
    {
        try
        {
            _systemSettings = ConfigManager.Instance.LoadConfig<SystemSettings>(Constants.SystemConfigFilePath);
            if (_systemSettings == null)
            {
                _systemSettings = new SystemSettings();
                Log.Warn("无法加载系统设置，使用默认设置");
            }
        }
        catch (Exception ex)
        {
            _systemSettings = new SystemSettings();
            Log.Error("加载系统设置失败，使用默认设置", ex);
        }
    }

    /// <summary>
    ///     定时器回调事件
    /// </summary>
    private async void OnMonitorTimerElapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            await Core.Utils.UIThreadHelper.InvokeAsync(() =>
            {
                UpdateDiskInfo();
                CheckDiskThreshold();
            });

            // await Task.Run(() =>
            // {
            //     UpdateDiskInfo();
            //     CheckDiskThreshold();
            // });
        }
        catch (Exception ex)
        {
            Log.Error("磁盘监控定时器回调失败", ex);
        }
    }

    /// <summary>
    ///     获取磁盘信息
    /// </summary>
    public List<DiskInfo> GetDiskInfo()
    {
        var diskInfos = new List<DiskInfo>();

        try
        {
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
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
        catch (Exception ex)
        {
            Log.Error("获取磁盘信息失败", ex);
        }

        return diskInfos;
    }

    /// <summary>
    ///     更新磁盘信息
    /// </summary>
    private void UpdateDiskInfo()
    {
        try
        {
            var diskInfos = GetDiskInfo();
            var targetDisk =
                diskInfos.FirstOrDefault(d => d.DriveName.StartsWith(Disk, StringComparison.OrdinalIgnoreCase));

            if (targetDisk != null)
            {
                TotalSizeGB = targetDisk.TotalSizeGB;
                AvailableSizeGB = targetDisk.AvailableSizeGB;
                UsagePercent = targetDisk.UsagePercent;

                // 更新显示文本
                DiskUsageText = $" {UsagePercent:F1}% ({AvailableSizeGB}GB可用)";

                // 更新状态颜色
                UpdateStatusColor();
            }
            else
            {
                // 磁盘不存在或不可用
                DiskUsageText = " 不可用";
                StatusBrush = new SolidColorBrush(Colors.Gray);
                Log.Warn($"磁盘 {Disk} 不存在或不可用");
            }
        }
        catch (Exception ex)
        {
            DiskUsageText = " 错误";
            StatusBrush = new SolidColorBrush(Colors.Red);
            Log.Error($"更新磁盘 {Disk} 信息失败", ex);
        }
    }

    /// <summary>
    ///     更新状态颜色
    /// </summary>
    private void UpdateStatusColor()
    {
        // 根据使用率设置颜色
        if (UsagePercent >= 95)
            StatusBrush = new SolidColorBrush(Colors.Red); // 严重警告
        else if (UsagePercent >= 85)
            StatusBrush = new SolidColorBrush(Colors.Orange); // 警告
        else if (UsagePercent >= 70)
            StatusBrush = new SolidColorBrush(Colors.Yellow); // 注意
        else
            StatusBrush = new SolidColorBrush(Colors.Green); // 正常
    }

    /// <summary>
    ///     检查磁盘阈值并触发报警
    /// </summary>
    private void CheckDiskThreshold()
    {
        if (_systemSettings == null || !_systemSettings.IsUseDiskWatcher)
            return;

        try
        {
            // 计算剩余空间百分比
            var remainPercent = TotalSizeGB > 0 ? (double)AvailableSizeGB / TotalSizeGB * 100 : 0;
            var thresholdPercent = _systemSettings.DiskRemainPct;

            // 检查是否低于阈值
            if (remainPercent <= thresholdPercent)
            {
                // 防止重复报警
                if (!_isAlarmTriggered)
                {
                    TriggerDiskAlarm(remainPercent, thresholdPercent);
                    _isAlarmTriggered = true;
                }
            }
            else
            {
                // 恢复正常，重置报警状态
                if (_isAlarmTriggered)
                {
                    ResolveDiskAlarm(remainPercent);
                    _isAlarmTriggered = false;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("检查磁盘阈值失败", ex);
        }
    }

    /// <summary>
    ///     触发磁盘空间不足报警
    /// </summary>
    private void TriggerDiskAlarm(double remainPercent, double thresholdPercent)
    {
        try
        {
            var alarmRecord = new AlarmRecord
            {
                Level = AlarmLevel.Warning,
                Module = "系统监控",
                Category = AlarmCategory.System,
                Message = $"磁盘 {Disk} 剩余空间仅为 {remainPercent:F1}%，低于设定阈值 {thresholdPercent}%",
                DataSnapshot = $"磁盘: {Disk}\n" +
                               $"总容量: {TotalSizeGB} GB\n" +
                               $"可用空间: {AvailableSizeGB} GB\n" +
                               $"剩余百分比: {remainPercent:F1}%\n" +
                               $"阈值: {thresholdPercent}%",
                Status = AlarmStatus.New,
                TriggerTime = DateTime.Now
            };

            // 通过报警协调器发送报警
            _ = Task.Run(async () => { await AlarmCoordinator.Instance.AlarmAsync(alarmRecord); });

            Log.Warn($"触发磁盘空间不足报警: {Disk} 剩余 {remainPercent:F1}%");
        }
        catch (Exception ex)
        {
            Log.Error("触发磁盘报警失败", ex);
        }
    }

    /// <summary>
    ///     解决磁盘空间报警
    /// </summary>
    private void ResolveDiskAlarm(double remainPercent)
    {
        try
        {
            var alarmRecord = new AlarmRecord
            {
                Level = AlarmLevel.Info,
                Module = "系统监控",
                Category = AlarmCategory.System,
                Message = $"磁盘 {Disk} 剩余空间已恢复到 {remainPercent:F1}%",
                DataSnapshot = $"磁盘: {Disk}\n" +
                               $"总容量: {TotalSizeGB} GB\n" +
                               $"可用空间: {AvailableSizeGB} GB\n" +
                               $"剩余百分比: {remainPercent:F1}%",
                Status = AlarmStatus.Resolved,
                TriggerTime = DateTime.Now,
                ResolvedTime = DateTime.Now,
                ResolvedBy = "系统自动"
            };

            // 通过报警协调器发送恢复通知
            _ = Task.Run(async () => { await AlarmCoordinator.Instance.ResolveAlarmAsync(alarmRecord); });

            Log.Info($"磁盘空间恢复正常: {Disk} 剩余 {remainPercent:F1}%");
        }
        catch (Exception ex)
        {
            Log.Error("解决磁盘报警失败", ex);
        }
    }

    #endregion

    #region IDisposable 实现

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     释放资源的具体实现
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            StopMonitoring();
            _monitorTimer?.Dispose();
            _disposed = true;

            Log.Debug($"磁盘监控视图模型已释放资源: {Disk}");
        }
    }

    /// <summary>
    ///     析构函数
    /// </summary>
    ~DiskMonitorViewModel()
    {
        Dispose(false);
    }

    #endregion
}