using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Interfaces;

/// <summary>
/// 配置管理器接口
/// 提供配置文件的读取、写入、缓存和热重载功能
/// </summary>
public interface IConfigManager
{
    /// <summary>
    /// 配置文件变更事件
    /// </summary>
    event EventHandler<ConfigChangedEventArgs>? ConfigChanged;

    /// <summary>
    /// 异步加载配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="configPath">配置文件路径</param>
    /// <param name="useCache">是否使用缓存，默认为true</param>
    /// <returns>配置对象</returns>
    Task<T?> LoadConfigAsync<T>(string configPath, bool useCache = true) where T : class;

    /// <summary>
    /// 异步保存配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="configPath">配置文件路径</param>
    /// <param name="config">配置对象</param>
    /// <param name="updateCache">是否更新缓存，默认为true</param>
    Task SaveConfigAsync<T>(string configPath, T config, bool updateCache = true) where T : class;

    /// <summary>
    /// 同步加载配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="configPath">配置文件路径</param>
    /// <param name="useCache">是否使用缓存，默认为true</param>
    /// <returns>配置对象</returns>
    T? LoadConfig<T>(string configPath, bool useCache = true) where T : class;

    Task<Dictionary<string, T?>> LoadConfigsAsync<T>(string directoryPath, bool useCache = true) where T : class;

    
    /// <summary>
    /// 同步保存配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="configPath">配置文件路径</param>
    /// <param name="config">配置对象</param>
    /// <param name="updateCache">是否更新缓存，默认为true</param>
    void SaveConfig<T>(string configPath, T config, bool updateCache = true) where T : class;

    /// <summary>
    /// 获取或创建配置（如果文件不存在则创建默认配置）
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="configPath">配置文件路径</param>
    /// <param name="defaultConfigFactory">默认配置工厂方法</param>
    /// <param name="useCache">是否使用缓存，默认为true</param>
    /// <returns>配置对象</returns>
    Task<T> GetOrCreateConfigAsync<T>(string configPath, Func<T> defaultConfigFactory, bool useCache = true) where T : class;

    /// <summary>
    /// 刷新配置缓存
    /// </summary>
    /// <param name="configPath">配置文件路径，如果为null则刷新所有缓存</param>
    Task RefreshCacheAsync(string? configPath = null);

    /// <summary>
    /// 清除配置缓存
    /// </summary>
    /// <param name="configPath">配置文件路径，如果为null则清除所有缓存</param>
    void ClearCache(string? configPath = null);

    /// <summary>
    /// 启用文件监控（热重载）
    /// </summary>
    /// <param name="configPath">配置文件路径</param>
    /// <param name="autoRefreshCache">是否自动刷新缓存，默认为true</param>
    void EnableFileWatcher(string configPath, bool autoRefreshCache = true);

    /// <summary>
    /// 禁用文件监控
    /// </summary>
    /// <param name="configPath">配置文件路径</param>
    void DisableFileWatcher(string configPath);

    /// <summary>
    /// 检查配置文件是否存在
    /// </summary>
    /// <param name="configPath">配置文件路径</param>
    /// <returns>文件是否存在</returns>
    bool ConfigExists(string configPath);

    /// <summary>
    /// 获取所有已缓存的配置路径
    /// </summary>
    /// <returns>配置路径列表</returns>
    IEnumerable<string> GetCachedConfigPaths();

    /// <summary>
    /// 验证配置对象
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="config">配置对象</param>
    /// <returns>验证结果</returns>
    ConfigValidationResult ValidateConfig<T>(T config) where T : class;

    /// <summary>
    /// 备份配置文件
    /// </summary>
    /// <param name="configPath">配置文件路径</param>
    /// <param name="backupPath">备份文件路径，如果为null则自动生成</param>
    /// <returns>备份文件路径</returns>
    Task<string> BackupConfigAsync(string configPath, string? backupPath = null);

    /// <summary>
    /// 从备份恢复配置文件
    /// </summary>
    /// <param name="configPath">配置文件路径</param>
    /// <param name="backupPath">备份文件路径</param>
    Task RestoreConfigAsync(string configPath, string backupPath);
}

/// <summary>
/// 配置变更事件参数
/// </summary>
public class ConfigChangedEventArgs : EventArgs
{
    /// <summary>
    /// 配置文件路径
    /// </summary>
    public string ConfigPath { get; }

    /// <summary>
    /// 配置类型
    /// </summary>
    public ConfigType ConfigType { get; }

    /// <summary>
    /// 变更类型
    /// </summary>
    public ConfigChangeType ChangeType { get; }

    /// <summary>
    /// 变更的属性名称列表
    /// </summary>
    public List<string> ChangedProperties { get; set; } = new();

    /// <summary>
    /// 变更时间
    /// </summary>
    public DateTime ChangeTime { get; }

    /// <summary>
    /// 变更前的配置对象（如果可用）
    /// </summary>
    public object? OldConfig { get; set; }

    /// <summary>
    /// 变更后的配置对象（如果可用）
    /// </summary>
    public object? NewConfig { get; set; }

    public ConfigChangedEventArgs(string configPath, ConfigType configType, ConfigChangeType changeType, List<string>? changedProperties = null)
    {
        ConfigPath = configPath;
        ConfigType = configType;
        ChangeType = changeType;
        ChangedProperties = changedProperties ?? new();
        ChangeTime = DateTime.Now;
    }
}

/// <summary>
/// 配置变更类型
/// </summary>
public enum ConfigChangeType
{
    /// <summary>
    /// 文件创建
    /// </summary>
    Created,

    /// <summary>
    /// 文件修改
    /// </summary>
    Modified,

    /// <summary>
    /// 文件删除
    /// </summary>
    Deleted,

    /// <summary>
    /// 文件重命名
    /// </summary>
    Renamed
}

/// <summary>
/// 配置文件类型枚举
/// </summary>
public enum ConfigType
{
    /// <summary>
    /// 未知配置类型
    /// </summary>
    Unknown,

    /// <summary>
    /// 系统配置
    /// </summary>
    System,

    /// <summary>
    /// 数据库配置
    /// </summary>
    Database,

    /// <summary>
    /// PLC配置
    /// </summary>
    Plc,

    /// <summary>
    /// 历史查询配置
    /// </summary>
    History,

    /// <summary>
    /// 产品配置
    /// </summary>
    Product,

    /// <summary>
    /// Imlight配置
    /// </summary>
    Imlight,

    /// <summary>
    /// DCE数据库配置
    /// </summary>
    DceDatabase,

    /// <summary>
    /// BOS数据库配置
    /// </summary>
    BosDatabase,

    /// <summary>
    /// WinTB3数据库配置
    /// </summary>
    WinTB3Database,
    /// <summary>
    /// 返修模块
    /// </summary>
    RepairModule,
    /// <summary>
    /// 拉铆模块
    /// </summary>
    RivetModule,
    SpotModule,
    ProjectionModule,
    ArcModule,
    DceModule,
}




