using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core;
using Core.Models.Settings;
using Core.Utils;
using SqlSugar;
using UI.Controls;

namespace MainApp.ViewModels.Settings;

public partial class DatabaseSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _availableCharsets = new();

    [ObservableProperty] private ObservableCollection<string> _availableDatabaseTypes = new();

    #region MySql配置

    /// <summary>
    ///     字符集
    /// </summary>
    [ObservableProperty] private string _charset = "utf8mb4";

    #endregion

    [ObservableProperty] private int _commandTimeout = 30;

    [ObservableProperty] private string _connectionString = string.Empty;

    [ObservableProperty] private int _connectionTimeout = 30;

    /// <summary>
    ///     数据库
    /// </summary>
    [ObservableProperty] private string _database = string.Empty;

    [ObservableProperty] private string _databaseType = "SQLite";

    [ObservableProperty] private bool _enableConnectionPooling = true;

    [ObservableProperty] private bool _enableTransactionLogging = true;

    [ObservableProperty] private string _host = string.Empty;

    [ObservableProperty] private string _instanceName = string.Empty;

    [ObservableProperty] private bool _isMySql;

    [ObservableProperty] private bool _isSqlServer;

    [ObservableProperty] private int _maxPoolSize = 100;

    [ObservableProperty] private int _minPoolSize = 5;

    [ObservableProperty] private string _password = string.Empty;

    private DatabaseSettings _settings;

    [ObservableProperty] private string _username = string.Empty;

    public DatabaseSettingsViewModel()
    {
        InitializeCollections();
    }

    /// <summary>
    ///     初始化集合数据
    /// </summary>
    private void InitializeCollections()
    {
        AvailableDatabaseTypes.Clear();
        AvailableDatabaseTypes.Add("SqlServer");
        AvailableDatabaseTypes.Add("MySQL");

        AvailableCharsets.Clear();
        AvailableCharsets.Add("UTF8");
        AvailableCharsets.Add("UTF8MB4");
        AvailableCharsets.Add("GBK");
        AvailableCharsets.Add("ASCII");
    }

    #region Command

    /// <summary>
    ///     测试数据库连接
    /// </summary>
    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        GenerateConnectionString();
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            MessageBox.Show("请先配置连接字符串", "提示");
            return;
        }

        try
        {
            var dbType = GetDbType(DatabaseType);
            var db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = ConnectionString,
                DbType = dbType,
                IsAutoCloseConnection = true
            });

            await Task.Run(() => db.Ado.CheckConnection());

            MessageBox.Success("数据库连接测试成功！", "连接测试");
        }
        catch (Exception ex)
        {
            MessageBox.Error($"数据库连接测试失败：{ex.Message}", "连接测试");
        }
    }

    #endregion

    /// <summary>
    ///     生成连接字符串
    /// </summary>
    [RelayCommand]
    private void GenerateConnectionString()
    {
        var defaultConnectionStrings = new Dictionary<string, string>
        {
            ["SQLite"] = "Data Source=./Data/scada.db",
            ["SqlServer"] = GenerateSqlServerConnectionString(),
            ["MySQL"] = GenerateMySqlConnectionString(),
            ["PostgreSQL"] = "Host=localhost;Database=ScadaDB;Username=postgres;Password=password;"
        };

        if (defaultConnectionStrings.TryGetValue(DatabaseType, out var defaultConnectionString))
            ConnectionString = defaultConnectionString;
    }

    /// <summary>
    ///     获取数据库类型
    /// </summary>
    private static DbType GetDbType(string databaseType)
    {
        return databaseType switch
        {
            "SQLite" => DbType.Sqlite,
            "SqlServer" => DbType.SqlServer,
            "MySQL" => DbType.MySql,
            "PostgreSQL" => DbType.PostgreSQL,
            _ => DbType.Sqlite
        };
    }

    /// <summary>
    ///     加载设置数据
    /// </summary>
    public void LoadSettings(DatabaseSettings settings)
    {
        _settings = settings;

        Host = settings.Host;
        Database = settings.Database;
        Username = settings.Username;
        Password = settings.Password;
        InstanceName = settings.InstanceName;
        UseIntegratedSecurity = settings.IntegratedSecurity;
        DatabaseType = settings.DatabaseType;
        ConnectionString = settings.ConnectionString;
        ConnectionTimeout = settings.ConnectionTimeout;
        CommandTimeout = settings.CommandTimeout;
        Charset = settings.Charset;
        EnableTransactionLogging = settings.EnableTransactionLogging;
    }

    /// <summary>
    ///     保存设置数据
    /// </summary>
    public async Task SaveSettings()
    {
        if (_settings == null) return;

        _settings.DatabaseType = DatabaseType;
        _settings.ConnectionString = ConnectionString;
        _settings.ConnectionTimeout = ConnectionTimeout;
        _settings.CommandTimeout = CommandTimeout;
        _settings.Username = Username;
        _settings.Password = Password;
        _settings.Host = Host;
        _settings.InstanceName = InstanceName;
        _settings.Database = Database;
        _settings.IntegratedSecurity = UseIntegratedSecurity;
        _settings.TrustServerCertificate = TrustServerCertificate;
        _settings.Charset = Charset;
        _settings.EnableTransactionLogging = EnableTransactionLogging;

        await ConfigManager.Instance.SaveConfigAsync(Constants.LocalDbConfigFilePath, _settings);
    }


    /// <summary>
    ///     验证设置
    /// </summary>
    public List<string> ValidateSettings()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ConnectionString))
            errors.Add("连接字符串不能为空");

        if (ConnectionTimeout < 1 || ConnectionTimeout > 300)
            errors.Add("连接超时时间必须在1-300秒之间");

        if (CommandTimeout < 1 || CommandTimeout > 300)
            errors.Add("命令超时时间必须在1-300秒之间");

        if (EnableConnectionPooling)
        {
            if (MaxPoolSize < 1 || MaxPoolSize > 1000)
                errors.Add("最大连接池大小必须在1-1000之间");

            if (MinPoolSize < 1 || MinPoolSize > MaxPoolSize)
                errors.Add("最小连接池大小必须在1到最大连接池大小之间");
        }

        return errors;
    }

    #region SQLServer配置

    /// <summary>
    ///     是否启用集成安全性
    /// </summary>
    [ObservableProperty] private bool _useIntegratedSecurity;


    [ObservableProperty] private bool _trustServerCertificate = true;

    /// <summary>
    ///     允许脏读
    /// </summary>
    [ObservableProperty] private bool _isWithNoLockQuery = true;

    #endregion

    #region 连接字符串工具函数

    /// <summary>
    ///     生成MySQL连接字符串
    /// </summary>
    /// <returns></returns>
    private string GenerateMySqlConnectionString()
    {
        return
            $"{GenerateCommonConnectionString()};Charset={Charset};Convert Zero Datetime=True;Allow Zero Datetime=True;";
    }

    /// <summary>
    ///     生成SQLServer连接字符串
    /// </summary>
    /// <returns></returns>
    private string GenerateSqlServerConnectionString()
    {
        return
            $"{GenerateCommonConnectionString()};Integrated Security={UseIntegratedSecurity};Encrypt=True;TrustServerCertificate=True;";
    }

    private string GenerateCommonConnectionString()
    {
        return $"server={Host};Database={Database};Uid={Username};Pwd={Password}";
    }

    #endregion

    #region 属性变化监听

    /// <summary>
    ///     属性变更时触发设置更改事件
    /// </summary>
    partial void OnDatabaseTypeChanged(string value)
    {
        IsMySql = value == "MySQL";
        IsSqlServer = value == "SqlServer";
        OnSettingsChanged();
    }

    partial void OnConnectionStringChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnConnectionTimeoutChanged(int value)
    {
        OnSettingsChanged();
    }

    partial void OnCommandTimeoutChanged(int value)
    {
        OnSettingsChanged();
    }

    partial void OnEnableConnectionPoolingChanged(bool value)
    {
        OnSettingsChanged();
    }

    partial void OnMaxPoolSizeChanged(int value)
    {
        OnSettingsChanged();
    }

    partial void OnMinPoolSizeChanged(int value)
    {
        OnSettingsChanged();
    }

    partial void OnHostChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnInstanceNameChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnDatabaseChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnUsernameChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnPasswordChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnTrustServerCertificateChanged(bool value)
    {
        OnSettingsChanged();
    }

    partial void OnCharsetChanged(string value)
    {
        OnSettingsChanged();
    }

    partial void OnEnableTransactionLoggingChanged(bool value)
    {
        OnSettingsChanged();
    }

    partial void OnUseIntegratedSecurityChanged(bool value)
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

    #endregion
}