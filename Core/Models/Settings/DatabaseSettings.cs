using System.ComponentModel.DataAnnotations;
using Core.Models;
using Data.SqlSugar;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using SqlSugar;

namespace Core.Models.Settings;

/// <summary>
/// 数据库设置配置
/// 包含数据库连接、性能优化、备份等配置
/// </summary>
public class DatabaseSettings : ConfigBase
{
    /// <summary>
    /// 数据库连接字符串
    /// </summary>
    [Required(ErrorMessage = "数据库连接字符串不能为空")]
    [JsonProperty("connectionString")]
    public string ConnectionString { get; set; } = "Data Source=scada.db;Version=3;";

    [JsonProperty("host")]
    public string Host { get; set; } = "localhost";
    
    [JsonProperty("instanceName")]
    public string InstanceName { get; set; } = "";
    
    [JsonProperty("database")]
    public string Database { get; set; } = "scada.db";
    
    [JsonProperty("username")]
    public string Username { get; set; } = "sa";
    
    [JsonProperty("password")]
    public string Password { get; set; } = "123456";
    
    /// <summary>
    /// 数据库类型（SQLite, SqlServer, MySQL, PostgreSQL）
    /// </summary>
    [Required(ErrorMessage = "数据库类型不能为空")]
    [JsonProperty("databaseType")]
    public string DatabaseType { get; set; } = "SqlServer";
    
    /// <summary>
    /// 连接超时时间（秒）
    /// </summary>
    [Range(1, 300, ErrorMessage = "连接超时时间必须在1-300秒之间")]
    [JsonProperty("connectionTimeout")]
    public int ConnectionTimeout { get; set; } = 10;

    /// <summary>
    /// 命令超时时间（秒）
    /// </summary>
    [Range(1, 600, ErrorMessage = "命令超时时间必须在1-600秒之间")]
    [JsonProperty("commandTimeout")]
    public int CommandTimeout { get; set; } = 60;

    /// <summary>
    /// MYSQL - 数据库字符编码
    /// </summary>
    [JsonProperty("charset")]
    public string Charset { get; set; } = "UTF8";

    /// <summary>
    /// 是否启用事务日志
    /// </summary>
    [JsonProperty("enableTransactionLogging")]
    public bool EnableTransactionLogging { get; set; } = true;

    /// <summary>
    /// SQL Server 是否启用集成安全性
    /// </summary>
    [JsonProperty("integratedSecurity")]
    public bool IntegratedSecurity { get; set; } = false;
    
    /// <summary>
    /// 是否信任服务器证书(默认true)
    /// </summary>
    [JsonProperty("trustServerCertificate")]
    public bool TrustServerCertificate { get; set; } = true;
    
    /// <summary>
    /// 是否启用日志(默认开启)
    /// </summary>
    [JsonProperty("enableLogging")]
    public bool EnableLogging { get; set; } = true;
    
    /// <summary>
    /// 是否启用驼峰转下划线命名
    /// </summary>
    [JsonProperty("enableUnderline")]
    public bool EnableUnderline { get; set; } = true;
    
    
    private DbType GetDbType()
    {
        return DatabaseType switch
        {
            "SQLite" => DbType.Sqlite,
            "SqlServer" => DbType.SqlServer,
            "MySQL" => DbType.MySql,
            "PostgreSQL" => DbType.PostgreSQL,
            _ => DbType.Sqlite
        };
    }
    
    public SqlSugarConfig ToSugarConfig()
    {
        SqlSugarConfig conf = new();

        conf.Key = "local";
        conf.Enabled = true;
        conf.Host = Host;
        conf.InstanceName = InstanceName;
        conf.Database = Database;
        conf.Username = Username;
        conf.Password = Password;
        conf.EnableLogging = EnableLogging;
        conf.EnableUnderline = EnableUnderline;
        conf.DbType = GetDbType();
        return conf;
    }
    
    public static DatabaseSettings FromSugarConfig(SqlSugarConfig conf)
    {
        return new DatabaseSettings
        {
            Host = conf.Host,
            InstanceName = conf.InstanceName,
            Database = conf.Database,
            Username = conf.Username,
            Password = conf.Password,
            EnableLogging = conf.EnableLogging,
            EnableUnderline = conf.EnableUnderline,
            DatabaseType = conf.DbType.ToString()
        };
    }
    
    /// <summary>
    /// 生成连接字符串
    /// </summary>
    public string ToSqlServerConnectionString()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = $@"{Host}\{InstanceName}",
            InitialCatalog = Database,
            ConnectTimeout = ConnectionTimeout,
            TrustServerCertificate = TrustServerCertificate,
            MultipleActiveResultSets = true
        };

        if (IntegratedSecurity)
        {
            builder.IntegratedSecurity = true;
            builder.UserID = string.Empty;
            builder.Password = string.Empty;
        }
        else
        {
            builder.IntegratedSecurity = false;
            builder.UserID = Username;
            builder.Password = Password;
        }

        return builder.ConnectionString;
    }
    
    public string ToMySqlConnectionString()
    {
        return $"server={Host};database={Database};user={Username};password={Password};charset=utf8mb4;convertzerotime=True";
    }

}
