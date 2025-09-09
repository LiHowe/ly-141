# ConfigManager 使用指南

## 概述

`ConfigManager` 是项目中的核心配置管理器，采用单例模式设计，提供了完整的配置文件管理功能，包括：

- 配置文件的读取、保存
- 缓存机制
- 文件监听和热重载
- 配置备份和恢复
- 配置验证

## 基本使用方法

### 获取单例实例

```csharp
// 获取ConfigManager的单例实例
var configManager = ConfigManager.Instance;
```

### 基础配置操作

#### 加载配置

```csharp
// 异步加载配置
var systemSettings = await ConfigManager.Instance.LoadConfigAsync<SystemSettings>("sys.conf");

// 同步加载配置
var systemSettings = ConfigManager.Instance.LoadConfig<SystemSettings>("sys.conf");

// 使用扩展方法加载系统配置（推荐）
var systemSettings = await ConfigManager.Instance.LoadSystemConfigAsync();
```

#### 保存配置

```csharp
// 异步保存配置
await ConfigManager.Instance.SaveConfigAsync("sys.conf", systemSettings);

// 同步保存配置
ConfigManager.Instance.SaveConfig("sys.conf", systemSettings);

// 使用扩展方法保存系统配置（推荐）
await ConfigManager.Instance.SaveSystemConfigAsync(systemSettings);
```

#### 获取或创建配置

```csharp
// 如果配置文件不存在，则创建默认配置
var systemSettings = await ConfigManager.Instance.GetOrCreateConfigAsync("sys.conf", () => new SystemSettings
{
    SystemName = "默认系统名称",
    SystemVersion = "1.0.0"
});
```

## 专用配置操作（推荐使用）

项目提供了专门的扩展方法来处理常用配置：

### 系统配置

```csharp
// 加载系统配置
var systemSettings = await ConfigManager.Instance.LoadSystemConfigAsync();

// 保存系统配置
await ConfigManager.Instance.SaveSystemConfigAsync(systemSettings);
```

### 数据库配置

```csharp
// 加载数据库配置
var dbSettings = await ConfigManager.Instance.LoadDbConfigAsync();

// 保存数据库配置
await ConfigManager.Instance.SaveDbConfigAsync(dbSettings);
```

### PLC配置

```csharp
// 加载PLC配置
var plcSettings = await ConfigManager.Instance.LoadPlcConfigAsync();

// 保存PLC配置
await ConfigManager.Instance.SavePlcConfigAsync(plcSettings);
```

## 配置监听和事件

### 订阅配置变更事件

```csharp
// 订阅配置变更事件
ConfigManager.Instance.ConfigChanged += OnConfigChanged;

private void OnConfigChanged(object? sender, ConfigChangedEventArgs e)
{
    switch (e.ConfigType)
    {
        case ConfigType.System:
            Console.WriteLine("系统配置已更改");
            break;
        case ConfigType.Database:
            Console.WriteLine("数据库配置已更改");
            break;
        case ConfigType.Plc:
            Console.WriteLine("PLC配置已更改");
            break;
    }
}
```

### 启用文件监听

```csharp
// 启用特定配置文件的监听
ConfigManager.Instance.EnableFileWatcher("sys.conf", autoRefreshCache: true);

// 禁用文件监听
ConfigManager.Instance.DisableFileWatcher("sys.conf");
```

## 缓存管理

### 刷新缓存

```csharp
// 刷新特定配置的缓存
await ConfigManager.Instance.RefreshCacheAsync("sys.conf");

// 刷新所有缓存
await ConfigManager.Instance.RefreshCacheAsync();
```

### 清除缓存

```csharp
// 清除特定配置的缓存
ConfigManager.Instance.ClearCache("sys.conf");

// 清除所有缓存
ConfigManager.Instance.ClearCache();
```

## 配置备份和恢复

### 备份配置

```csharp
// 备份配置文件
var backupPath = await ConfigManager.Instance.BackupConfigAsync("sys.conf");
Console.WriteLine($"配置已备份到: {backupPath}");

// 指定备份路径
var customBackupPath = await ConfigManager.Instance.BackupConfigAsync("sys.conf", "backup/sys_backup.conf");
```

### 恢复配置

```csharp
// 从备份恢复配置
await ConfigManager.Instance.RestoreConfigAsync("sys.conf", backupPath);
```

## 配置验证

```csharp
// 验证配置
var validationResult = ConfigManager.Instance.ValidateConfig(systemSettings);

if (!validationResult.IsValid)
{
    Console.WriteLine($"配置验证失败: {string.Join(", ", validationResult.Errors)}");
}
```

## 在不同场景中的使用

### 在窗口中使用

```csharp
public partial class SettingsWindow : Window
{
    private readonly IConfigManager _configManager;

    public SettingsWindow()
    {
        InitializeComponent();
        _configManager = ConfigManager.Instance;
        
        // 订阅配置变更事件
        _configManager.ConfigChanged += OnConfigChanged;
    }

    private async void LoadSettings()
    {
        var systemSettings = await _configManager.LoadSystemConfigAsync();
        // 更新UI
    }

    private async void SaveSettings()
    {
        var systemSettings = new SystemSettings { /* 设置属性 */ };
        await _configManager.SaveSystemConfigAsync(systemSettings);
    }

    private void OnConfigChanged(object? sender, ConfigChangedEventArgs e)
    {
        if (e.ConfigType == ConfigType.System)
        {
            Dispatcher.Invoke(async () =>
            {
                await LoadSettings(); // 重新加载设置
            });
        }
    }
}
```

### 在ViewModel中使用

```csharp
public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IConfigManager _configManager;

    public MainWindowViewModel()
    {
        _configManager = ConfigManager.Instance;
        
        // 加载初始配置
        LoadConfiguration();
        
        // 订阅配置变更
        _configManager.ConfigChanged += OnConfigurationChanged;
    }

    private async void LoadConfiguration()
    {
        var systemSettings = await _configManager.LoadSystemConfigAsync();
        Title = systemSettings.SystemName;
        Version = systemSettings.SystemVersion;
    }

    private void OnConfigurationChanged(object? sender, ConfigChangedEventArgs e)
    {
        if (e.ConfigType == ConfigType.System)
        {
            LoadConfiguration();
        }
    }
}
```

### 在服务类中使用

```csharp
public class DataService
{
    private readonly IConfigManager _configManager;

    public DataService()
    {
        _configManager = ConfigManager.Instance;
    }

    public async Task<string> GetConnectionStringAsync()
    {
        var dbSettings = await _configManager.LoadDbConfigAsync();
        return dbSettings.ConnectionString;
    }

    public async Task UpdateDatabaseConfigAsync(DatabaseSettings newSettings)
    {
        await _configManager.SaveDbConfigAsync(newSettings);
    }
}
```

## 配置管理器选项

### 设置配置选项

```csharp
var options = new ConfigManagerOptions
{
    DefaultConfigRoot = "config",
    EnableCache = true,
    EnableFileWatcher = true,
    EnableBackup = true,
    MaxBackupFiles = 5,
    CacheExpirationMinutes = 60,
    AutoCreateDirectory = true
};

ConfigManager.Instance.ApplyOptions(options);
```

## 最佳实践

1. **使用扩展方法**: 优先使用 `LoadSystemConfigAsync()`, `SaveSystemConfigAsync()` 等扩展方法，而不是通用的 `LoadConfigAsync<T>()`。

2. **异步操作**: 尽量使用异步方法，避免阻塞UI线程。

3. **事件订阅**: 在需要响应配置变更的地方订阅 `ConfigChanged` 事件。

4. **资源清理**: 在不需要时取消事件订阅，避免内存泄漏。

5. **错误处理**: 始终包装配置操作在 try-catch 块中。

## 注意事项

1. **线程安全**: `ConfigManager` 是线程安全的单例。

2. **文件监听**: 启用文件监听后，配置文件的外部更改会自动触发事件。

3. **缓存机制**: 默认启用缓存，可以提高性能，但需要注意缓存一致性。

4. **备份管理**: 自动备份功能会保留指定数量的备份文件，旧备份会被自动清理。

这个配置管理器提供了完整的配置管理解决方案，简化了配置操作，提高了代码的可维护性。
