using System.Text.Json.Serialization;

namespace Core.Models;

/// <summary>
/// 模块配置信息
/// </summary>
public class ModuleConfiguration
{
    /// <summary>
    /// 模块ID
    /// </summary>
    [JsonPropertyName("moduleId")]
    public string ModuleId { get; set; } = string.Empty;

    /// <summary>
    /// 模块名称
    /// </summary>
    [JsonPropertyName("moduleName")]
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// 模块是否启用
    /// </summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 模块加载优先级（数值越小优先级越高）
    /// </summary>
    [JsonPropertyName("priority")]
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 模块程序集文件路径（相对于Modules目录）
    /// </summary>
    [JsonPropertyName("assemblyPath")]
    public string? AssemblyPath { get; set; }

    /// <summary>
    /// 模块类型全名
    /// </summary>
    [JsonPropertyName("moduleTypeName")]
    public string? ModuleTypeName { get; set; }

    /// <summary>
    /// 模块描述
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 模块版本
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// 是否为内置模块
    /// </summary>
    [JsonPropertyName("isBuiltIn")]
    public bool IsBuiltIn { get; set; } = false;

    /// <summary>
    /// 模块依赖项
    /// </summary>
    [JsonPropertyName("dependencies")]
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// 模块配置参数
    /// </summary>
    [JsonPropertyName("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// 模块数据库配置
    /// </summary>
    [JsonPropertyName("database")]
    public ModuleDatabaseConfiguration? DatabaseConfiguration { get; set; }
}

/// <summary>
/// 模块数据库配置
/// </summary>
public class ModuleDatabaseConfiguration
{
    /// <summary>
    /// 模块ID
    /// </summary>
    [JsonPropertyName("moduleId")]
    public string ModuleId { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用数据库功能
    /// </summary>
    [JsonPropertyName("enableDatabase")]
    public bool EnableDatabase { get; set; } = true;

    /// <summary>
    /// 数据库连接字符串（可选，为空则使用系统默认）
    /// </summary>
    [JsonPropertyName("connectionString")]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 数据库类型（可选，为空则使用系统默认）
    /// </summary>
    [JsonPropertyName("databaseType")]
    public string? DatabaseType { get; set; }

    /// <summary>
    /// 表名前缀
    /// </summary>
    [JsonPropertyName("tablePrefix")]
    public string TablePrefix { get; set; } = string.Empty;

    /// <summary>
    /// 是否自动创建表
    /// </summary>
    [JsonPropertyName("autoCreateTables")]
    public bool AutoCreateTables { get; set; } = true;

    /// <summary>
    /// 是否在模块卸载时删除表
    /// </summary>
    [JsonPropertyName("dropTablesOnUnload")]
    public bool DropTablesOnUnload { get; set; } = false;

    /// <summary>
    /// 数据库迁移设置
    /// </summary>
    [JsonPropertyName("migrationSettings")]
    public ModuleDatabaseMigrationSettings MigrationSettings { get; set; } = new();
}

/// <summary>
/// 模块数据库迁移设置
/// </summary>
public class ModuleDatabaseMigrationSettings
{
    /// <summary>
    /// 是否启用自动迁移
    /// </summary>
    [JsonPropertyName("enableAutoMigration")]
    public bool EnableAutoMigration { get; set; } = true;

    /// <summary>
    /// 迁移脚本目录
    /// </summary>
    [JsonPropertyName("migrationScriptsPath")]
    public string MigrationScriptsPath { get; set; } = "Migrations";

    /// <summary>
    /// 备份数据库
    /// </summary>
    [JsonPropertyName("backupBeforeMigration")]
    public bool BackupBeforeMigration { get; set; } = true;
}

/// <summary>
/// 模块配置文件根对象
/// </summary>
public class ModuleConfigurationRoot
{
    /// <summary>
    /// 配置文件版本
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// 是否启用动态模块加载
    /// </summary>
    [JsonPropertyName("enableDynamicLoading")]
    public bool EnableDynamicLoading { get; set; } = true;

    /// <summary>
    /// 模块加载超时时间（秒）
    /// </summary>
    [JsonPropertyName("loadTimeoutSeconds")]
    public int LoadTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 模块配置列表
    /// </summary>
    [JsonPropertyName("modules")]
    public List<ModuleConfiguration> Modules { get; set; } = new();
}
