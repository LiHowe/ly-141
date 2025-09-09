using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Core;
using Core.Models.Settings;
using Core.Utils;

namespace MainApp.ViewModels.Settings;

public partial class SystemSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _availableDisks = new();

    [ObservableProperty] private ObservableCollection<string> _availableLogLevels = new();

    [ObservableProperty] private ObservableCollection<string> _availableRunModes = new();

    [ObservableProperty] private int _diskRemainPct = 20;

    [ObservableProperty] private bool _isDefaultFullScreen;

    /// <summary>
    ///     是否隐藏任务栏
    /// </summary>
    [ObservableProperty] private bool _isHideTaskbar;

    [ObservableProperty] private bool _isMinimizeToTray;

    /// <summary>
    ///     是否显示开发者选项
    /// </summary>
    [ObservableProperty] private bool _isShowDevPanel;

    [ObservableProperty] private bool _isTablesInitialized;

    /// <summary>
    ///     是否使用自动启动
    /// </summary>
    [ObservableProperty] private bool _isUseAutoStart;

    [ObservableProperty] private bool _isUseDiskWatcher;

    [ObservableProperty] private string _logLevel = "Info";

    [ObservableProperty] private int _maxLogFiles = 30;

    [ObservableProperty] private string _runMode = "生产模式";

    private SystemSettings _settings;

    [ObservableProperty] private string _systemName = "维美数据采集与追溯系统";

    [ObservableProperty] private string _systemSubName = "WinM Data Acquisition and Traceability System";

    [ObservableProperty] private string _systemVersion = "3.0.0";

    [ObservableProperty] private string _watchedDisk = "C";


    public SystemSettingsViewModel()
    {
        var sysConfig = ConfigManager.Instance.LoadConfig<SystemSettings>(Constants.SystemConfigFilePath);
        LoadSettings(sysConfig);

        InitializeCollections();
    }

    /// <summary>
    ///     初始化集合数据
    /// </summary>
    private void InitializeCollections()
    {
        AvailableLogLevels.Clear();
        AvailableLogLevels.Add("Debug");
        AvailableLogLevels.Add("Info");
        AvailableLogLevels.Add("Warning");
        AvailableLogLevels.Add("Error");
        AvailableLogLevels.Add("Fatal");

        AvailableRunModes.Clear();
        AvailableRunModes.Add("生产模式");
        AvailableRunModes.Add("调试模式");
        AvailableRunModes.Add("演示模式");

        AvailableDisks.Clear();
        DriveInfo.GetDrives()
            .Where(d => d.IsReady)
            .Select(d => d.Name[0].ToString())
            .ToList()
            .ForEach(AvailableDisks.Add);
    }

    /// <summary>
    ///     加载设置数据
    /// </summary>
    public void LoadSettings(SystemSettings sysConfig)
    {
        _settings = sysConfig;

        IsUseAutoStart = sysConfig.IsUseAutoStart;
        IsHideTaskbar = sysConfig.IsHideTaskbar;
        IsDefaultFullScreen = sysConfig.IsDefaultFullScreen;
        IsMinimizeToTray = sysConfig.IsMinimizeToTray;
        IsUseDiskWatcher = sysConfig.IsUseDiskWatcher;
        DiskRemainPct = sysConfig.DiskRemainPct;
        WatchedDisk = sysConfig.WatchedDisk;
        SystemName = sysConfig.SystemName;
        SystemSubName = sysConfig.SystemSubName;
        SystemVersion = sysConfig.SystemVersion;
        LogLevel = sysConfig.LogLevel;
        MaxLogFiles = sysConfig.MaxLogFiles;
        RunMode = sysConfig.RunMode;
        IsTablesInitialized = sysConfig.IsTablesInitialized;
    }

    public async Task SaveSettings()
    {
        if (_settings == null) return;
        _settings.IsUseAutoStart = IsUseAutoStart;
        _settings.IsHideTaskbar = IsHideTaskbar;
        _settings.IsDefaultFullScreen = IsDefaultFullScreen;
        _settings.IsMinimizeToTray = IsMinimizeToTray;
        _settings.IsUseDiskWatcher = IsUseDiskWatcher;
        _settings.DiskRemainPct = DiskRemainPct;
        _settings.WatchedDisk = WatchedDisk;
        _settings.SystemName = SystemName;
        _settings.SystemSubName = SystemSubName;
        _settings.SystemVersion = SystemVersion;
        _settings.LogLevel = LogLevel;
        _settings.MaxLogFiles = MaxLogFiles;
        _settings.RunMode = RunMode;
        _settings.IsTablesInitialized = IsTablesInitialized;
        await ConfigManager.Instance.SaveSystemConfigAsync(_settings);
    }

    partial void OnIsUseDiskWatcherChanged(bool value)
    {
        OnSettingsChanged();
    }

    partial void OnDiskRemainPctChanged(int value)
    {
        OnSettingsChanged();
    }

    partial void OnWatchedDiskChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnSystemNameChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnSystemSubNameChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnSystemVersionChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnLogLevelChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnMaxLogFilesChanged(int value)
    {
        OnSettingsChanged();
    }

    partial void OnRunModeChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnIsUseAutoStartChanged(bool value)
    {
        OnSettingsChanged();
    }

    partial void OnIsHideTaskbarChanged(bool value)
    {
        OnSettingsChanged();
    }

    partial void OnIsDefaultFullScreenChanged(bool value)
    {
        OnSettingsChanged();
    }

    partial void OnIsMinimizeToTrayChanged(bool value)
    {
        OnSettingsChanged();
    }


    /// <summary>
    ///     设置更改事件
    /// </summary>
    public event EventHandler? SettingsChanged;

    /// <summary>
    ///     触发设置更改事件
    /// </summary>
    private void OnSettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }
}