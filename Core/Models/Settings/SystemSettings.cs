using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Core.Models.Settings;

public class SystemSettings : ConfigBase
{
    /// <summary>
    /// 系统名称
    /// </summary>
    [Required(ErrorMessage = "系统名称不能为空")]
    [JsonProperty("systemName")]
    public string SystemName { get; set; } = "维美数据采集与追溯系统";

    /// <summary>
    /// 系统副标题
    /// </summary>
    [Required(ErrorMessage = "系统名称不能为空")]
    [JsonProperty("systemSubName")]
    public string SystemSubName { get; set; } = "WinM Data Acquisition and Traceability System";

    /// <summary>
    /// 系统版本
    /// </summary>
    [JsonProperty("systemVersion")]
    public string SystemVersion { get; set; } = "3.0.0";

    /// <summary>
    /// 日志级别
    /// </summary>
    [JsonProperty("logLevel")]
    public string LogLevel { get; set; } = "Info";

    /// <summary>
    /// 最大日志文件数, 0 为无限制
    /// </summary>
    [Range(0, 100, ErrorMessage = "最大日志文件数必须在1-100之间")]
    [JsonProperty("maxLogFiles")]
    public int MaxLogFiles { get; set; } = 30;

    /// <summary>
    /// 系统运行模式（生产模式, 调试模式, 演示模式）
    /// </summary>
    [JsonProperty("runMode")]
    public string RunMode { get; set; } = "生产模式";

    /// <summary>
    /// 系统是否已经初始化了数据库
    /// </summary>
    [JsonProperty("isTableInitialized")]
    public bool IsTablesInitialized { get; set; } = false;

    /// <summary>
    /// 是否启用看门狗
    /// </summary>
    [JsonProperty("isUseWatchDog")]
    public bool IsUseWatchDog { get; set; } = true;
    
    /// <summary>
    /// 是否启用磁盘监控
    /// </summary>
    [JsonProperty("isUseDiskWatcher")]
    public bool IsUseDiskWatcher { get; set; } = true;
    
    /// <summary>
    ///  磁盘剩余空间阈值（百分比）
    /// </summary>
    [JsonProperty("diskRemainPct")]
    public int DiskRemainPct { get; set; } = 20;

    /// <summary>
    ///  监控磁盘
    /// </summary>
    [JsonProperty("watchedDisk")]
    public string WatchedDisk { get; set; } = "d";
    
    /// <summary>
    ///  是否隐藏任务栏
    /// </summary>
    [JsonProperty("isHideTaskbar")]
    public bool IsHideTaskbar { get; set; } = false;
    
    /// <summary>
    ///  是否使用自动启动
    /// </summary>
    [JsonProperty("isUseAutoStart")]
    public bool IsUseAutoStart { get; set; } = false;
    
    /// <summary>
    ///  是否默认全屏
    /// </summary>
    [JsonProperty("isDefaultFullScreen")]
    public bool IsDefaultFullScreen { get; set; } = false;

    /// <summary>
    ///  是否最小化到托盘
    /// </summary>
    [JsonProperty("isMinimizeToTray")]
    public bool IsMinimizeToTray { get; set; } = false;
}