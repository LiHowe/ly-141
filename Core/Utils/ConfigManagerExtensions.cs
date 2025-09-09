using Core.Interfaces;
using Core.Models.Settings;
using Data.SqlSugar;
using Connection.S7;
using System.Collections.ObjectModel;

namespace Core.Utils;

public static class ConfigManagerExtensions
{
    /// <summary>
    /// 保存系统配置
    /// </summary>
    /// <param name="configManager">配置管理器</param>
    /// <param name="settings">系统配置</param>
    /// <param name="configPath">配置文件路径，默认使用Constants中定义的路径</param>
    public static async Task SaveSystemConfigAsync(this IConfigManager configManager, SystemSettings settings, string configPath = "")
    {
        if (string.IsNullOrWhiteSpace(configPath)) configPath = Constants.SystemConfigFilePath;
        await configManager.SaveConfigAsync(configPath, settings);
    }

    /// <summary>
    /// 加载系统设置
    /// </summary>
    /// <param name="configManager">配置管理器</param>
    /// <param name="configPath">配置文件路径，默认使用Constants中定义的路径</param>
    /// <returns>系统设置</returns>
    public static async Task<SystemSettings> LoadSystemConfigAsync(this IConfigManager configManager, string configPath = "")
    {
        if (string.IsNullOrWhiteSpace(configPath)) configPath = Constants.SystemConfigFilePath;
        return await configManager.GetOrCreateConfigAsync(configPath, () => new SystemSettings
        {
            SystemName = "WinmTech-Scada",
            SystemVersion = "1.0.0",
            LogLevel = "Info",
            MaxLogFiles = 10,
            RunMode = "生产模式",
            Description = "系统基本设置"
        });
    }
    /// <summary>
    /// 加载数据库配置
    /// </summary>
    /// <param name="configManager">配置管理器</param>
    /// <param name="path">配置文件路径，默认使用Constants中定义的路径</param>
    /// <returns>数据库配置</returns>
    public static async Task<DatabaseSettings> LoadDbConfigAsync(this IConfigManager configManager, string path = "")
    {
        if (string.IsNullOrWhiteSpace(path)) path = Constants.LocalDbConfigFilePath;
        return await configManager.GetOrCreateConfigAsync(path, () => new DatabaseSettings
        {
            ConnectionString = "server=localhost;Database=winm_mes;Uid=sa;Pwd=123456;Encrypt=True;TrustServerCertificate=True;",
            DatabaseType = "SqlServer",
            ConnectionTimeout = 30,
            CommandTimeout = 60,
            Description = "数据库连接和性能设置",
            Username = "sa",
            Password = "123456",
            Host = "localhost",
            InstanceName = "",
            Database = "winm_mes"
        });
    }
    
    public static DatabaseSettings LoadDbConfig(this IConfigManager configManager, string path = "")
    {
        if (string.IsNullOrWhiteSpace(path)) path = Constants.LocalDbConfigFilePath;
        return configManager.LoadConfig<DatabaseSettings>(path);
    }

    /// <summary>
    /// 保存数据库配置
    /// </summary>
    /// <param name="configManager">配置管理器</param>
    /// <param name="settings">数据库配置</param>
    /// <param name="path">配置文件路径，默认使用Constants中定义的路径</param>
    public static async Task SaveDbConfigAsync(this IConfigManager configManager, DatabaseSettings settings, string path = "")
    {
        if (string.IsNullOrWhiteSpace(path)) path = Constants.LocalDbConfigFilePath;
        await configManager.SaveConfigAsync(path, settings);
    }

    /// <summary>
    /// 加载PLC配置
    /// </summary>
    /// <param name="configManager">配置管理器</param>
    /// <param name="path">配置文件路径，默认使用Constants中定义的路径</param>
    /// <returns>PLC配置</returns>
    public static async Task<PlcSettings> LoadPlcConfigAsync(this IConfigManager configManager, string path = "")
    {
         if (string.IsNullOrWhiteSpace(path)) path = Constants.PlcConfigFilePath;
         return await configManager.GetOrCreateConfigAsync(path, () => new PlcSettings
         {
             Configs = new(new S7PlcConfig[]
             {
                 new S7PlcConfig
                 {
                     Key = "plc_1",
                     Name = "PLC1",
                     Type = "S7-1500",
                     Ip = "192.168.31.100",
                     Rack = 0,
                     Slot = 1,
                     Enabled = true,
                     Description = "PLC1",
                     Nodes = new ObservableCollection<S7PlcNode>()
                 }
             })
         });
    }

    /// <summary>
    /// 保存PLC配置
    /// </summary>
    /// <param name="configManager">配置管理器</param>
    /// <param name="settings">PLC配置</param>
    /// <param name="path">配置文件路径，默认使用Constants中定义的路径</param>
    public static async Task SavePlcConfigAsync(this IConfigManager configManager, PlcSettings settings, string path = "")
    {
        if (string.IsNullOrWhiteSpace(path)) path = Constants.PlcConfigFilePath;
        await configManager.SaveConfigAsync(path, settings);
    }
    
}