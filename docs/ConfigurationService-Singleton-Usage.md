# ConfigurationService 单例模式使用指南

## 概述

`ConfigurationService` 现在已经改为单例模式，确保在整个应用程序中只有一个配置服务实例。这样可以：

- 避免重复初始化配置
- 确保配置状态的一致性
- 减少内存占用
- 提供全局访问点

## 基本使用方法

### 获取单例实例

```csharp
// 获取ConfigurationService的单例实例
var configService = ConfigurationService.Instance;
```

### 读取配置

```csharp
// 获取系统设置
var systemSettings = ConfigurationService.Instance.GetSystemSettings();
Console.WriteLine($"系统名称: {systemSettings.SystemName}");

// 获取数据库设置
var dbSettings = ConfigurationService.Instance.GetDatabaseSettings();
Console.WriteLine($"数据库连接字符串: {dbSettings.ConnectionString}");

// 获取PLC设置
var plcSettings = ConfigurationService.Instance.GetPlcSettings();
Console.WriteLine($"PLC配置数量: {plcSettings.Configs.Count}");
```

### 更新配置

```csharp
// 异步更新系统设置
var newSystemSettings = new SystemSettings
{
    SystemName = "新的系统名称",
    SystemVersion = "2.0.0",
    LogLevel = "Debug"
};
await ConfigurationService.Instance.UpdateSystemSettingsAsync(newSystemSettings);

// 异步更新数据库设置
var newDbSettings = new DatabaseSettings
{
    ConnectionString = "新的连接字符串",
    DatabaseType = "SqlServer",
    ConnectionTimeout = 60
};
await ConfigurationService.Instance.UpdateDatabaseSettingsAsync(newDbSettings);
```

### 订阅配置变更事件

```csharp
// 订阅系统配置变更事件
ConfigurationService.Instance.SystemConfigurationChanged += (sender, newSettings) =>
{
    Console.WriteLine($"系统配置已更新: {newSettings.SystemName}");
};

// 订阅数据库配置变更事件
ConfigurationService.Instance.DatabaseConfigurationChanged += (sender, newSettings) =>
{
    Console.WriteLine($"数据库配置已更新: {newSettings.DatabaseType}");
};

// 订阅PLC配置变更事件
ConfigurationService.Instance.PlcConfigurationChanged += (sender, newSettings) =>
{
    Console.WriteLine($"PLC配置已更新，配置数量: {newSettings.Configs.Count}");
};
```

## 在不同场景中的使用

### 在窗口中使用

```csharp
public partial class SettingsWindow : Window
{
    private readonly ConfigurationService _configService;

    public SettingsWindow()
    {
        InitializeComponent();
        
        // 使用单例实例
        _configService = ConfigurationService.Instance;
        
        // 订阅配置变更事件
        _configService.SystemConfigurationChanged += OnSystemConfigurationChanged;
        _configService.DatabaseConfigurationChanged += OnDatabaseConfigurationChanged;
        _configService.PlcConfigurationChanged += OnPlcConfigurationChanged;
    }

    private void OnSystemConfigurationChanged(object? sender, SystemSettings settings)
    {
        // 处理系统配置变更
        Dispatcher.Invoke(() =>
        {
            // 更新UI
        });
    }
}
```

### 在ViewModel中使用

```csharp
public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ConfigurationService _configService;

    public MainWindowViewModel()
    {
        _configService = ConfigurationService.Instance;
        
        // 加载初始配置
        LoadConfiguration();
        
        // 订阅配置变更
        _configService.SystemConfigurationChanged += OnConfigurationChanged;
    }

    private void LoadConfiguration()
    {
        var systemSettings = _configService.GetSystemSettings();
        Title = systemSettings.SystemName;
        Version = systemSettings.SystemVersion;
    }

    private void OnConfigurationChanged(object? sender, SystemSettings settings)
    {
        // 更新ViewModel属性
        Title = settings.SystemName;
        Version = settings.SystemVersion;
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Version));
    }
}
```

### 在服务类中使用

```csharp
public class DataService
{
    private readonly ConfigurationService _configService;

    public DataService()
    {
        _configService = ConfigurationService.Instance;
    }

    public async Task<string> GetConnectionStringAsync()
    {
        var dbSettings = _configService.GetDatabaseSettings();
        return dbSettings.ConnectionString;
    }

    public async Task UpdateDatabaseConfigAsync(string newConnectionString)
    {
        var currentSettings = _configService.GetDatabaseSettings();
        currentSettings.ConnectionString = newConnectionString;
        
        await _configService.UpdateDatabaseSettingsAsync(currentSettings);
    }
}
```

## 配置备份和恢复

```csharp
// 备份配置
var systemBackupPath = await ConfigurationService.Instance.BackupSystemConfigurationAsync();
var dbBackupPath = await ConfigurationService.Instance.BackupDatabaseConfigurationAsync();
var plcBackupPath = await ConfigurationService.Instance.BackupPlcConfigurationAsync();

Console.WriteLine($"系统配置备份到: {systemBackupPath}");
Console.WriteLine($"数据库配置备份到: {dbBackupPath}");
Console.WriteLine($"PLC配置备份到: {plcBackupPath}");

// 从备份恢复配置
await ConfigurationService.Instance.RestoreSystemConfigurationAsync(systemBackupPath);
await ConfigurationService.Instance.RestoreDatabaseConfigurationAsync(dbBackupPath);
await ConfigurationService.Instance.RestorePlcConfigurationAsync(plcBackupPath);
```

## 配置验证

```csharp
// 验证各种配置
var systemValidation = ConfigurationService.Instance.ValidateSystemConfiguration();
var dbValidation = ConfigurationService.Instance.ValidateDatabaseConfiguration();
var plcValidation = ConfigurationService.Instance.ValidatePlcConfiguration();

if (!systemValidation.IsValid)
{
    Console.WriteLine($"系统配置验证失败: {string.Join(", ", systemValidation.Errors)}");
}

if (!dbValidation.IsValid)
{
    Console.WriteLine($"数据库配置验证失败: {string.Join(", ", dbValidation.Errors)}");
}

if (!plcValidation.IsValid)
{
    Console.WriteLine($"PLC配置验证失败: {string.Join(", ", plcValidation.Errors)}");
}
```

## 应用程序生命周期管理

### 应用程序启动时

```csharp
// 在App.xaml.cs中
protected override async void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    
    // ConfigurationService会在第一次访问Instance时自动初始化
    // 无需手动初始化
    
    await ApplicationInitializer.Initialize();
}
```

### 应用程序关闭时

```csharp
// 在App.xaml.cs中
protected override void OnExit(ExitEventArgs e)
{
    // 释放ConfigurationService单例实例
    ConfigurationService.DisposeInstance();
    
    base.OnExit(e);
}
```

## 注意事项

1. **线程安全**: 单例实现使用了双重检查锁定模式，确保线程安全。

2. **自动初始化**: 第一次访问 `Instance` 属性时会自动创建和初始化实例。

3. **事件订阅**: 由于是单例，事件订阅会在整个应用程序生命周期内保持有效。

4. **资源释放**: 应用程序关闭时调用 `DisposeInstance()` 确保资源正确释放。

5. **配置热重载**: 单例实例支持配置文件的热重载，当配置文件发生变化时会自动触发相应事件。

## 迁移指南

如果您之前使用的是非单例版本，需要进行以下更改：

### 之前的用法
```csharp
// 旧的用法 - 不再推荐
var configService = new ConfigurationService();
```

### 现在的用法
```csharp
// 新的用法 - 推荐
var configService = ConfigurationService.Instance;
```

这样的更改确保了整个应用程序中配置服务的一致性和高效性。
