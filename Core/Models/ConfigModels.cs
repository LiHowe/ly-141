using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Core.Models;

/// <summary>
/// 配置缓存项
/// </summary>
internal class ConfigCacheItem
{
    /// <summary>
    /// 配置对象
    /// </summary>
    public object? Config { get; set; }

    /// <summary>
    /// 配置类型
    /// </summary>
    public Type ConfigType { get; set; } = typeof(object);

    /// <summary>
    /// 缓存时间
    /// </summary>
    public DateTime CacheTime { get; set; }

    /// <summary>
    /// 文件最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用文件监控
    /// </summary>
    public bool IsWatcherEnabled { get; set; }
}

/// <summary>
/// 配置管理器选项
/// </summary>
public class ConfigManagerOptions
{
    /// <summary>
    /// 默认配置根目录
    /// </summary>
    public string DefaultConfigRoot { get; set; } = Constants.ConfigRootPath;

    /// <summary>
    /// 是否启用缓存，默认为true
    /// </summary>
    public bool EnableCache { get; set; } = true;

    /// <summary>
    /// 缓存过期时间（分钟），0表示永不过期
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 0;

    /// <summary>
    /// 是否启用文件监控，默认为true
    /// </summary>
    public bool EnableFileWatcher { get; set; } = true;

    /// <summary>
    /// 文件监控延迟（毫秒），防止频繁触发
    /// </summary>
    public int FileWatcherDelayMs { get; set; } = 500;

    /// <summary>
    /// 是否自动创建配置目录
    /// </summary>
    public bool AutoCreateDirectory { get; set; } = true;

    /// <summary>
    /// 是否启用配置备份
    /// </summary>
    public bool EnableBackup { get; set; } = true;

    /// <summary>
    /// 备份文件保留数量
    /// </summary>
    public int MaxBackupFiles { get; set; } = 5;

    /// <summary>
    /// JSON序列化设置
    /// </summary>
    public JsonSerializerSettings JsonSettings { get; set; } = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Include,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        DateTimeZoneHandling = DateTimeZoneHandling.Local
    };
}

/// <summary>
/// 配置基类，提供通用的配置属性和验证
/// </summary>
public abstract class ConfigBase
{
    /// <summary>
    /// 配置版本
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// 配置创建时间
    /// </summary>
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 配置最后修改时间
    /// </summary>
    [JsonProperty("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.Now;

    /// <summary>
    /// 配置描述
    /// </summary>
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 验证配置
    /// </summary>
    /// <returns>验证结果</returns>
    public virtual ConfigValidationResult Validate()
    {
        var context = new ValidationContext(this);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = Validator.TryValidateObject(this, context, results, true);

        return new ConfigValidationResult
        {
            IsValid = isValid,
            Errors = results.Select(r => r.ErrorMessage ?? "未知错误").ToList()
        };
    }

    /// <summary>
    /// 更新最后修改时间
    /// </summary>
    public virtual void UpdateLastModified()
    {
        LastModified = DateTime.Now;
    }
}

/// <summary>
/// 配置验证结果
/// </summary>
public class ConfigValidationResult
{
    /// <summary>
    /// 验证是否成功
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 错误信息列表
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 警告信息列表
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// 添加错误信息
    /// </summary>
    /// <param name="error">错误信息</param>
    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    /// <summary>
    /// 添加警告信息
    /// </summary>
    /// <param name="warning">警告信息</param>
    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}

/// <summary>
/// 示例配置类 - 简单应用程序配置
/// 用于演示ConfigManager的基本使用
/// </summary>
public class SimpleAppConfig : ConfigBase
{
    /// <summary>
    /// 应用程序名称
    /// </summary>
    [Required(ErrorMessage = "应用程序名称不能为空")]
    [JsonProperty("appName")]
    public string AppName { get; set; } = "WinmTech-Scada";

    /// <summary>
    /// 应用程序版本
    /// </summary>
    [JsonProperty("appVersion")]
    public string AppVersion { get; set; } = "1.0.0";

    /// <summary>
    /// 语言设置
    /// </summary>
    [JsonProperty("language")]
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// 主题设置
    /// </summary>
    [JsonProperty("theme")]
    public string Theme { get; set; } = "Default";

    /// <summary>
    /// 是否启用自动保存
    /// </summary>
    [JsonProperty("autoSave")]
    public bool AutoSave { get; set; } = true;

    /// <summary>
    /// 自动保存间隔（秒）
    /// </summary>
    [Range(10, 3600, ErrorMessage = "自动保存间隔必须在10-3600秒之间")]
    [JsonProperty("autoSaveInterval")]
    public int AutoSaveInterval { get; set; } = 300;
}
