using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using Core.Interfaces;
using Core.Models;
using Core.Services;
using Logger;
using Newtonsoft.Json;

namespace Core.Utils;

/// <summary>
/// 配置管理器实现类
/// 提供配置文件的读取、写入、缓存和热重载功能
/// </summary>
public class ConfigManager : IConfigManager, IDisposable
{
    private ConfigManagerOptions _options;
    private ConcurrentDictionary<string, ConfigCacheItem> _cache;
    private ConcurrentDictionary<string, FileSystemWatcher> _watchers;
    private ConcurrentDictionary<string, Timer> _watcherTimers;
    private readonly object _lockObject = new();
    private bool _disposed = false;

    
    // 使用 Lazy<T> 确保线程安全的单例模式
    private static readonly Lazy<ConfigManager> _instance = new Lazy<ConfigManager>(() => new ConfigManager());

    // 配置文件缓存
    private readonly Dictionary<string, object> _configCache = new();

    // 私有构造函数，防止外部实例化
    private ConfigManager()
    {
        _cache = new ConcurrentDictionary<string, ConfigCacheItem>();
        _watchers = new ConcurrentDictionary<string, FileSystemWatcher>();
        _watcherTimers = new ConcurrentDictionary<string, Timer>();
        _options = new ConfigManagerOptions();
        // 确保默认配置目录存在
        if (_options.AutoCreateDirectory && !string.IsNullOrEmpty(_options.DefaultConfigRoot))
        {
            Directory.CreateDirectory(_options.DefaultConfigRoot);
        }

        if (_options.EnableFileWatcher)
        {
            EnableFileWatcher(Constants.SystemConfigFilePath);
            EnableFileWatcher(Constants.PlcConfigFilePath);
            EnableFileWatcher(Constants.LocalDbConfigFilePath);
        }
    }

    // 获取 ConfigManager 单例实例
    public static ConfigManager Instance => _instance.Value;
    
    /// <summary>
    /// 配置文件变更事件
    /// </summary>
    public event EventHandler<ConfigChangedEventArgs>? ConfigChanged;
    

    /// <summary>
    /// 更改配置管理器配置
    /// </summary>
    /// <param name="options"></param>
    public void ApplyOptions(ConfigManagerOptions options)
    {
        _options = options;

        // 确保默认配置目录存在
        if (_options.AutoCreateDirectory && !string.IsNullOrEmpty(_options.DefaultConfigRoot))
        {
            Directory.CreateDirectory(_options.DefaultConfigRoot);
        }
    }

    /// <summary>
    /// 异步加载配置
    /// </summary>
    public async Task<T?> LoadConfigAsync<T>(string configPath, bool useCache = true) where T : class
    {
        return await Task.Run(() => LoadConfig<T>(configPath, useCache));
    }
    /// <summary>
    /// 同步加载配置
    /// </summary>
    public T? LoadConfig<T>(string configPath, bool useCache = true) where T : class
    {
        try
        {
            var fullPath = GetFullPath(configPath);

            // 检查缓存
            if (useCache && _options.EnableCache && _cache.TryGetValue(fullPath, out var cacheItem))
            {
                if (IsCacheValid(cacheItem, fullPath))
                {
                    return cacheItem.Config as T;
                }
                else
                {
                    // 缓存过期，移除
                    _cache.TryRemove(fullPath, out _);
                }
            }

            // 检查文件是否存在
            if (!File.Exists(fullPath))
            {
                return null;
            }

            // 读取并解析配置文件
            var json = File.ReadAllText(fullPath);
            var config = JsonConvert.DeserializeObject<T>(json, _options.JsonSettings);

            // 更新缓存
            if (useCache && _options.EnableCache && config != null)
            {
                var newCacheItem = new ConfigCacheItem
                {
                    Config = config,
                    ConfigType = typeof(T),
                    CacheTime = DateTime.Now,
                    LastModified = File.GetLastWriteTime(fullPath),
                    FilePath = fullPath,
                    IsWatcherEnabled = false
                };

                _cache.AddOrUpdate(fullPath, newCacheItem, (key, oldValue) => newCacheItem);
            }

            return config;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"加载配置文件失败: {configPath}", ex);
        }
    }
    
    /// <summary>
    /// 加载某一文件夹下的全部配置文件
    /// </summary>
    /// <typeparam name="T">配置对象类型</typeparam>
    /// <param name="directoryPath">文件夹路径</param>
    /// <param name="useCache">是否使用缓存</param>
    /// <returns>以配置文件的Key为键，配置对象为值的字典</returns>
    public async Task<Dictionary<string, T?>> LoadConfigsAsync<T>(string directoryPath, bool useCache = true) where T : class
    {
        return await Task.Run(() => LoadConfigs<T>(directoryPath, useCache));
    }

    /// <summary>
    /// 同步加载某一文件夹下的全部配置文件
    /// </summary>
    /// <typeparam name="T">配置对象类型</typeparam>
    /// <param name="directoryPath">文件夹路径</param>
    /// <param name="useCache">是否使用缓存</param>
    /// <returns>以配置文件的Key为键，配置对象为值的字典</returns>
    public Dictionary<string, T?> LoadConfigs<T>(string directoryPath, bool useCache = true) where T : class
    {
        var result = new Dictionary<string, T?>();

        try
        {
            // 获取目录下所有的配置文件，支持.json和.conf扩展名
            var jsonFiles = Directory.GetFiles(directoryPath, "*.json");
            var confFiles = Directory.GetFiles(directoryPath, $"*.{Constants.ConfigExt}");
            var files = jsonFiles.Concat(confFiles).ToArray();

            foreach (var file in files)
            {
                var config = LoadConfig<T>(file, useCache); // 加载每个配置文件

                if (config != null)
                {
                    var keyProperty = config.GetType().GetProperty("Key");
                    if (keyProperty != null)
                    {
                        var key = keyProperty.GetValue(config)?.ToString();
                        if (!string.IsNullOrEmpty(key))
                        {
                            result[key] = config; // 根据Key存入字典
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"加载配置文件夹失败: {directoryPath}", ex);
        }

        return result;
    }


    /// <summary>
    /// 异步保存配置
    /// </summary>
    public async Task SaveConfigAsync<T>(string configPath, T config, bool updateCache = true) where T : class
    {
        await Task.Run(() => SaveConfig(configPath, config, updateCache));
    }

    /// <summary>
    /// 同步保存配置
    /// </summary>
    public void SaveConfig<T>(string configPath, T config, bool updateCache = true) where T : class
    {
        try
        {
            var fullPath = GetFullPath(configPath);

            // 确保目录存在
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 更新最后修改时间（如果配置继承自ConfigBase）
            if (config is ConfigBase configBase)
            {
                configBase.UpdateLastModified();
            }

            // 备份现有文件
            if (_options.EnableBackup && File.Exists(fullPath))
            {
                BackupConfigAsync(configPath).Wait();
            }

            // 序列化并保存
            var json = JsonConvert.SerializeObject(config, _options.JsonSettings);
            File.WriteAllText(fullPath, json);

            // 更新缓存
            if (updateCache && _options.EnableCache)
            {
                var cacheItem = new ConfigCacheItem
                {
                    Config = config,
                    ConfigType = typeof(T),
                    CacheTime = DateTime.Now,
                    LastModified = File.GetLastWriteTime(fullPath),
                    FilePath = fullPath,
                    IsWatcherEnabled = _watchers.ContainsKey(fullPath)
                };

                _cache.AddOrUpdate(fullPath, cacheItem, (key, oldValue) => cacheItem);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"保存配置文件失败: {configPath}", ex);
        }
    }

    /// <summary>
    /// 获取或创建配置
    /// </summary>
    public async Task<T> GetOrCreateConfigAsync<T>(string configPath, Func<T> defaultConfigFactory, bool useCache = true) where T : class
    {
        var config = await LoadConfigAsync<T>(configPath, useCache);
        if (config != null)
        {
            return config;
        }

        // 创建默认配置
        config = defaultConfigFactory();
        await SaveConfigAsync(configPath, config, useCache);
        return config;
    }

    /// <summary>
    /// 刷新配置缓存
    /// </summary>
    public async Task RefreshCacheAsync(string? configPath = null)
    {
        await Task.Run(() =>
        {
            if (configPath != null)
            {
                var fullPath = GetFullPath(configPath);
                if (_cache.TryGetValue(fullPath, out var cacheItem))
                {
                    _cache.TryRemove(fullPath, out _);
                    // 重新加载配置
                    var method = typeof(ConfigManager).GetMethod(nameof(LoadConfig))?.MakeGenericMethod(cacheItem.ConfigType);
                    method?.Invoke(this, new object[] { configPath, true });
                }
            }
            else
            {
                // 刷新所有缓存
                var paths = _cache.Keys.ToList();
                foreach (var path in paths)
                {
                    if (_cache.TryGetValue(path, out var cacheItem))
                    {
                        _cache.TryRemove(path, out _);
                        var relativePath = GetRelativePath(path);
                        var method = typeof(ConfigManager).GetMethod(nameof(LoadConfig))?.MakeGenericMethod(cacheItem.ConfigType);
                        method?.Invoke(this, new object[] { relativePath, true });
                    }
                }
            }
        });
    }

    /// <summary>
    /// 清除配置缓存
    /// </summary>
    public void ClearCache(string? configPath = null)
    {
        if (configPath != null)
        {
            var fullPath = GetFullPath(configPath);
            _cache.TryRemove(fullPath, out _);
        }
        else
        {
            _cache.Clear();
        }
    }

    /// <summary>
    /// 启用文件监控
    /// </summary>
    public void EnableFileWatcher(string configPath, bool autoRefreshCache = true)
    {
        if (!_options.EnableFileWatcher) return;

        var fullPath = GetFullPath(configPath);
        var directory = Path.GetDirectoryName(fullPath);
        var fileName = Path.GetFileName(fullPath);

        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName)) return;

        lock (_lockObject)
        {
            if (_watchers.ContainsKey(fullPath)) return;

            var watcher = new FileSystemWatcher(directory, fileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            watcher.Changed += (sender, e) => OnFileChanged(e.FullPath, ConfigChangeType.Modified, autoRefreshCache);
            watcher.Created += (sender, e) => OnFileChanged(e.FullPath, ConfigChangeType.Created, autoRefreshCache);
            watcher.Deleted += (sender, e) => OnFileChanged(e.FullPath, ConfigChangeType.Deleted, autoRefreshCache);
            watcher.Renamed += (sender, e) => OnFileChanged(e.FullPath, ConfigChangeType.Renamed, autoRefreshCache);

            _watchers.TryAdd(fullPath, watcher);

            // 更新缓存项的监控状态
            if (_cache.TryGetValue(fullPath, out var cacheItem))
            {
                cacheItem.IsWatcherEnabled = true;
            }
        }
    }

    /// <summary>
    /// 禁用文件监控
    /// </summary>
    public void DisableFileWatcher(string configPath)
    {
        var fullPath = GetFullPath(configPath);

        lock (_lockObject)
        {
            if (_watchers.TryRemove(fullPath, out var watcher))
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            if (_watcherTimers.TryRemove(fullPath, out var timer))
            {
                timer.Dispose();
            }

            // 更新缓存项的监控状态
            if (_cache.TryGetValue(fullPath, out var cacheItem))
            {
                cacheItem.IsWatcherEnabled = false;
            }
        }
    }

    /// <summary>
    /// 检查配置文件是否存在
    /// </summary>
    public bool ConfigExists(string configPath)
    {
        var fullPath = GetFullPath(configPath);
        return File.Exists(fullPath);
    }

    /// <summary>
    /// 获取所有已缓存的配置路径
    /// </summary>
    public IEnumerable<string> GetCachedConfigPaths()
    {
        return _cache.Keys.Select(GetRelativePath);
    }

    /// <summary>
    /// 验证配置对象
    /// </summary>
    public ConfigValidationResult ValidateConfig<T>(T config) where T : class
    {
        var result = new ConfigValidationResult { IsValid = true };

        try
        {
            var context = new ValidationContext(config);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var isValid = Validator.TryValidateObject(config, context, validationResults, true);

            if (!isValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(validationResults.Select(vr => vr.ErrorMessage ?? "未知验证错误"));
            }

            // 如果配置继承自ConfigBase，调用其验证方法
            if (config is ConfigBase configBase)
            {
                var baseValidation = configBase.Validate();
                if (!baseValidation.IsValid)
                {
                    result.IsValid = false;
                    result.Errors.AddRange(baseValidation.Errors);
                }
            }
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.AddError($"验证过程中发生错误: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// 备份配置文件
    /// </summary>
    public async Task<string> BackupConfigAsync(string configPath, string? backupPath = null)
    {
        return await Task.Run(() =>
        {
            var fullPath = GetFullPath(configPath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"配置文件不存在: {configPath}");
            }

            if (string.IsNullOrEmpty(backupPath))
            {
                var directory = Path.GetDirectoryName(fullPath) ?? "";
                var fileName = Path.GetFileNameWithoutExtension(fullPath);
                var extension = Path.GetExtension(fullPath);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                backupPath = Path.Combine(directory, $"{fileName}_backup_{timestamp}{extension}");
            }

            File.Copy(fullPath, backupPath, true);

            // 清理旧备份文件
            CleanupOldBackups(configPath);

            return backupPath;
        });
    }

    /// <summary>
    /// 从备份恢复配置文件
    /// </summary>
    public async Task RestoreConfigAsync(string configPath, string backupPath)
    {
        await Task.Run(() =>
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException($"备份文件不存在: {backupPath}");
            }

            var fullPath = GetFullPath(configPath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(backupPath, fullPath, true);

            // 清除缓存，强制重新加载
            ClearCache(configPath);
        });
    }

    /// <summary>
    /// 获取完整路径
    /// </summary>
    private string GetFullPath(string configPath)
    {
        if (Path.IsPathRooted(configPath))
        {
            return configPath;
        }

        return Path.Combine(_options.DefaultConfigRoot, configPath);
    }

    /// <summary>
    /// 获取相对路径
    /// </summary>
    private string GetRelativePath(string fullPath)
    {
        if (fullPath.StartsWith(_options.DefaultConfigRoot))
        {
            return Path.GetRelativePath(_options.DefaultConfigRoot, fullPath);
        }
        return fullPath;
    }

    /// <summary>
    /// 检查缓存是否有效
    /// </summary>
    private bool IsCacheValid(ConfigCacheItem cacheItem, string fullPath)
    {
        // 检查缓存过期时间
        if (_options.CacheExpirationMinutes > 0)
        {
            var expiration = cacheItem.CacheTime.AddMinutes(_options.CacheExpirationMinutes);
            if (DateTime.Now > expiration)
            {
                return false;
            }
        }

        // 检查文件是否被修改
        if (File.Exists(fullPath))
        {
            var lastModified = File.GetLastWriteTime(fullPath);
            return lastModified <= cacheItem.LastModified;
        }

        return false;
    }

    /// <summary>
    /// 文件变更事件处理
    /// </summary>
    private void OnFileChanged(string fullPath, ConfigChangeType changeType, bool autoRefreshCache)
    {
        // 使用定时器防止频繁触发
        if (_watcherTimers.TryGetValue(fullPath, out var existingTimer))
        {
            existingTimer.Dispose();
        }

        var timer = new Timer(async _ =>
        {
            try
            {
                var relativePath = GetRelativePath(fullPath);
                var configType = ConfigTypeHelper.GetConfigTypeByConstants(relativePath);

                object? oldConfig = null;
                object? newConfig = null;
                List<string> changedProperties = new();

                // 获取变更前的配置（如果缓存中存在）
                if (_cache.TryGetValue(fullPath, out var cacheItem))
                {
                    oldConfig = cacheItem.Config;
                }

                if (autoRefreshCache)
                {
                    await RefreshCacheAsync(fullPath);

                    // 获取变更后的配置
                    if (_cache.TryGetValue(fullPath, out var newCacheItem))
                    {
                        newConfig = newCacheItem.Config;
                    }
                }

                // 检测属性变更
                if (oldConfig != null && newConfig != null && oldConfig.GetType() == newConfig.GetType())
                {
                    changedProperties = GetChangedPropertiesForConfig(oldConfig, newConfig);
                }

                // 触发配置变更事件
                var eventArgs = new ConfigChangedEventArgs(fullPath, configType, changeType, changedProperties)
                {
                    OldConfig = oldConfig,
                    NewConfig = newConfig
                };
                ConfigChanged?.Invoke(this, eventArgs);
            }
            catch (Exception ex)
            {
                // 记录错误但不抛出异常
                System.Diagnostics.Debug.WriteLine($"处理文件变更事件时发生错误: {ex.Message}");
                Log.Error("处理配置文件变更事件时发生错误", ex);
            }
            finally
            {
                _watcherTimers.TryRemove(fullPath, out var timerToDispose);
                timerToDispose?.Dispose();
            }
        }, null, _options.FileWatcherDelayMs, Timeout.Infinite);

        _watcherTimers.TryAdd(fullPath, timer);
    }


    /// <summary>
    /// 获取配置对象的属性变更列表
    /// </summary>
    private List<string> GetChangedPropertiesForConfig(object oldConfig, object newConfig)
    {
        try
        {
            // 使用反射动态调用ConfigDiffHelper.GetChangedProperties方法
            var method = typeof(ConfigDiffHelper).GetMethod("GetChangedProperties");
            if (method != null)
            {
                var genericMethod = method.MakeGenericMethod(oldConfig.GetType());
                var result = genericMethod.Invoke(null, new[] { oldConfig, newConfig });
                return result as List<string> ?? new List<string>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"检测配置属性变更时发生错误: {ex.Message}");
        }

        return new List<string>();
    }

    /// <summary>
    /// 清理旧备份文件
    /// </summary>
    private void CleanupOldBackups(string configPath)
    {
        try
        {
            var fullPath = GetFullPath(configPath);
            var directory = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileNameWithoutExtension(fullPath);
            var extension = Path.GetExtension(fullPath);

            if (string.IsNullOrEmpty(directory)) return;

            var backupPattern = $"{fileName}_backup_*{extension}";
            var backupFiles = Directory.GetFiles(directory, backupPattern)
                .OrderByDescending(f => File.GetCreationTime(f))
                .Skip(_options.MaxBackupFiles)
                .ToList();

            foreach (var backupFile in backupFiles)
            {
                try
                {
                    File.Delete(backupFile);
                }
                catch
                {
                    // 忽略删除失败的情况
                }
            }
        }
        catch
        {
            // 忽略清理过程中的错误
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        // 停止所有文件监控
        foreach (var watcher in _watchers.Values)
        {
            try
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            catch
            {
                // 忽略释放过程中的错误
            }
        }

        // 释放所有定时器
        foreach (var timer in _watcherTimers.Values)
        {
            try
            {
                timer.Dispose();
            }
            catch
            {
                // 忽略释放过程中的错误
            }
        }

        _watchers.Clear();
        _watcherTimers.Clear();
        _cache.Clear();

        _disposed = true;
    }
}
