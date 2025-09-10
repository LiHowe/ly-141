using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Connection.S7;
using Core;
using Core.Interfaces;
using Core.Localization;
using Core.Messages;
using Core.Models.Settings;
using Core.Services;
using Core.Utils;
using Logger;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using UI.Controls;

namespace MainApp.ViewModels;

/// <summary>
///     主窗体视图模型
/// </summary>
public partial class MainWindowViewModel : ObservableObject, IDisposable, IRecipient<CultureChangedMessage>
{
    private readonly ConcurrentDictionary<DeviceConnectionViewModel, S7Plc> _plcMap = new();

    private readonly Timer _systemTimeTimer;

    [ObservableProperty] private ObservableCollection<DeviceConnectionViewModel> _connections = new();

    private Timer _deviceStatusTimer;

    /// <summary>
    ///     磁盘监控视图模型
    /// </summary>
    [ObservableProperty] private DiskMonitorViewModel? _diskMonitorViewModel;

    private bool _disposed;

    /// <summary>
    ///     是否启用磁盘监控
    /// </summary>
    [ObservableProperty] private bool _enableDiskMonitor = true;

    [ObservableProperty] private string _subTitle;

    [ObservableProperty] private string _systemTime;
    [ObservableProperty] private string _title;

    public MainWindowViewModel()
    {
        // 实时更新系统时间
        _systemTimeTimer =
            new Timer(state => { SystemTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); }, null, 0,
                1000);
		// 注册消息
		WeakReferenceMessenger.Default.Register<CultureChangedMessage>(this);
		// 标题与系统设置进行绑定
		var sysConfig = ConfigManager.Instance.LoadConfig<SystemSettings>(Constants.SystemConfigFilePath);
        EnableDiskMonitor = sysConfig.IsUseDiskWatcher;
        Title = sysConfig.SystemName;
        SubTitle = sysConfig.SystemSubName;
        ConfigManager.Instance.ConfigChanged += OnConfigChanged;

        if (EnableDiskMonitor)
            // 初始化磁盘监控
            InitializeDiskMonitor(sysConfig);

        InitDeviceStatusBar();
    }

	// 实现 IRecipient 接口，处理语言切换消息
	public void Receive(CultureChangedMessage message)
	{
		// 可选择更新 ViewModel 特定属性
		// 例如：OnPropertyChanged(nameof(SomeProperty));
	}

	private async Task InitDeviceStatusBar()
    {
        var plcConfig = await ConfigManager.Instance.LoadPlcConfigAsync();
        // 每5秒检测设备连接状态
        if (_deviceStatusTimer != null) _deviceStatusTimer.Dispose();
        _deviceStatusTimer = new Timer(state =>
        {
            if (_plcMap.Count == 0) return;
            UIThreadHelper.InvokeAsync(() =>
            {
                foreach (var pair in _plcMap)
                {
                    var isConnected = pair.Value.IsConnected;
                    pair.Key.Status = isConnected ? DeviceConnectionStatus.Normal : DeviceConnectionStatus.Disconnect;
                    if (!isConnected)
                        try
                        {
                            pair.Value.ConnectAsync();
                        }
                        catch (Exception ex)
                        {
                            pair.Key.Status = DeviceConnectionStatus.Error;
                        }
                }
            });
        }, null, 0, 5 * 1000);
        await UIThreadHelper.InvokeAsync(() =>
        {
            Connections.Clear();
            foreach (var config in plcConfig.Configs)
            {
                if (!config.Enabled) continue;
                var vm = new DeviceConnectionViewModel
                {
                    Name = config.Name,
                    Ip = config.Ip
                };


                var plc = config.GetPlc();
                _plcMap[vm] = plc;
                plc.OnStoped += (s, e) =>
                {
                    UIThreadHelper.InvokeAsync(() => { vm.Status = DeviceConnectionStatus.Error; });
                };

                plc.OnRunning += (s, e) =>
                {
                    UIThreadHelper.InvokeAsync(() => { vm.Status = DeviceConnectionStatus.Normal; });
                };

                plc.OnDisconnected += (s, e) =>
                {
                    UIThreadHelper.InvokeAsync(() => { vm.Status = DeviceConnectionStatus.Disconnect; });
                };

                plc.OnStatusChanged += (s, e) => { Console.WriteLine(e.ToString()); };

                vm.Clicked += async (s, e) =>
                {
                    try
                    {
                        Console.WriteLine($"点击了{vm.Name}");
                        if (plc.IsConnected)
                            vm.Status = DeviceConnectionStatus.Normal;
                        // await plc.DisconnectAsync();
                        else
                            await plc.ConnectAsync();
                    }
                    catch (Exception ex)
                    {
                        // ignored
                        vm.Status = DeviceConnectionStatus.Error;
                    }
                };

                Connections.Add(vm);

                Task.Run(() =>
                {
                    try
                    {
                        // plc.UseAutoReconnection();
                        plc.ConnectAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"PLC {plc.Name} 连接失败", ex);
                    }
                });
            }
        });
    }

    [RelayCommand]
    private async Task SwitchLanguage()
    {
        await UIThreadHelper.InvokeAsync(() =>
        {
			//LocalizationService.Instance.CurrentCulture = cultureCode ?? "zh-CN";
			string cultureCode =  LocalizationProvider.Default.Culture.Name == "zh-CN" ? "fr-FR" : "zh-CN";
			App.SwitchLanguage(cultureCode);
   //         Core.Properties.Settings.Default.SelectedCulture = cultureCode;
			//Core.Properties.Settings.Default.Save();
        });
	}

    private void OnConfigChanged(object? sender, ConfigChangedEventArgs e)
    {
        // 只处理系统配置的变更
        if (e.ConfigType == ConfigType.System && e.ChangeType == ConfigChangeType.Modified)
        {
            var sysConfig = ConfigManager.Instance.LoadSystemConfigAsync().GetAwaiter().GetResult();
            UIThreadHelper.InvokeAsync(() =>
            {
                Title = sysConfig.SystemName;
                SubTitle = sysConfig.SystemSubName;
            });


            // 检查磁盘监控相关配置是否变更
            // if (e.ChangedProperties.Contains("IsUseDiskWatcher") ||
            //     e.ChangedProperties.Contains("WatchedDisk") ||
            //     e.ChangedProperties.Contains("DiskRemainPct"))
            // {
            //     UIThreadHelper.InvokeAsync(() =>
            //     {
            //         EnableDiskMonitor = sysConfig.IsUseDiskWatcher;
            //         UpdateDiskMonitorConfig(sysConfig);
            //     });
            // }

            UIThreadHelper.InvokeAsync(() =>
            {
                EnableDiskMonitor = sysConfig.IsUseDiskWatcher;
                UpdateDiskMonitorConfig(sysConfig);
            });

            // 记录变更的属性
            if (e.ChangedProperties.Count > 0)
                Debug.WriteLine($"系统配置变更，影响属性: {string.Join(", ", e.ChangedProperties)}");
        }
        else if (e.ConfigType == ConfigType.Plc && e.ChangeType == ConfigChangeType.Modified)
        {
            UIThreadHelper.InvokeAsync(async () => { await InitDeviceStatusBar(); });
        }
    }

    /// <summary>
    ///     初始化磁盘监控
    /// </summary>
    /// <param name="sysConfig">系统配置</param>
    private void InitializeDiskMonitor(SystemSettings sysConfig)
    {
        try
        {
            // 如果启用了磁盘监控
            if (sysConfig.IsUseDiskWatcher)
            {
                DiskMonitorViewModel = new DiskMonitorViewModel();

                var diskToMonitor = "C";
                // 设置监控的磁盘盘符
                if (!string.IsNullOrEmpty(sysConfig.WatchedDisk) && sysConfig.WatchedDisk.Length > 0)
                    diskToMonitor = sysConfig.WatchedDisk[0].ToString();

                DiskMonitorViewModel.SetDisk(diskToMonitor);

                // 启动监控
                DiskMonitorViewModel.StartMonitoring();

                Log.Info($"磁盘监控已启动，监控磁盘: {diskToMonitor}");
            }
            else
            {
                DiskMonitorViewModel?.StopMonitoring();
                DiskMonitorViewModel = null;
                Log.Info("磁盘监控已禁用");
            }
        }
        catch (Exception ex)
        {
            Log.Error("初始化磁盘监控失败", ex);
            DiskMonitorViewModel = null;
        }
    }

    /// <summary>
    ///     更新磁盘监控配置
    /// </summary>
    /// <param name="sysConfig">系统配置</param>
    private void UpdateDiskMonitorConfig(SystemSettings sysConfig)
    {
        try
        {
            // 如果启用了磁盘监控
            if (sysConfig.IsUseDiskWatcher)
            {
                // 如果之前没有磁盘监控，创建新的
                if (DiskMonitorViewModel == null) DiskMonitorViewModel = new DiskMonitorViewModel();
                var diskToMonitor = "C";
                // 设置监控的磁盘盘符
                if (!string.IsNullOrEmpty(sysConfig.WatchedDisk) && sysConfig.WatchedDisk.Length > 0)
                    diskToMonitor = sysConfig.WatchedDisk[0].ToString();

                if (diskToMonitor == DiskMonitorViewModel.Disk) return;

                // 停止当前监控
                DiskMonitorViewModel.StopMonitoring();

                // 设置新的监控磁盘
                DiskMonitorViewModel.SetDisk(diskToMonitor);

                // 重新启动监控
                DiskMonitorViewModel.StartMonitoring();

                Log.Info($"磁盘监控配置已更新，监控磁盘: {diskToMonitor}");
            }
            else
            {
                // 如果禁用了磁盘监控，停止并清理
                if (DiskMonitorViewModel != null)
                {
                    DiskMonitorViewModel.StopMonitoring();
                    DiskMonitorViewModel.Dispose();
                    DiskMonitorViewModel = null;

                    Log.Info("磁盘监控已禁用");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("更新磁盘监控配置失败", ex);
        }
    }

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
            try
            {
                // 停止系统时间定时器
                _systemTimeTimer?.Dispose();

                // 清理磁盘监控
                if (DiskMonitorViewModel != null)
                {
                    DiskMonitorViewModel.StopMonitoring();
                    DiskMonitorViewModel.Dispose();
                    DiskMonitorViewModel = null;
                }

                // 取消配置变更事件订阅
                ConfigManager.Instance.ConfigChanged -= OnConfigChanged;

                _disposed = true;
                Log.Debug("MainWindowViewModel 资源已释放");
            }
            catch (Exception ex)
            {
                Log.Error("释放 MainWindowViewModel 资源时发生错误", ex);
            }
    }

    /// <summary>
    ///     析构函数
    /// </summary>
    ~MainWindowViewModel()
    {
        Dispose(false);
    }

    #endregion
}