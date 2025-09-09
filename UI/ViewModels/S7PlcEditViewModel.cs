using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Connection.S7;

namespace UI.ViewModels;

public partial class S7PlcEditViewModel : ObservableObject
{
    #region 集合属性

    /// <summary>
    ///     可用的 PLC 型号列表
    /// </summary>
    [ObservableProperty] private ObservableCollection<string> _availableTypes = new()
    {
        "S7-200",
        "S7-300",
        "S7-400",
        "S7-1200",
        "S7-1500"
    };

    #endregion

    #region 基本属性

    /// <summary>
    ///     PLC 标识符
    /// </summary>
    [ObservableProperty] private string _key = string.Empty;

    /// <summary>
    ///     PLC 名称
    /// </summary>
    [ObservableProperty] private string _title = string.Empty;

    /// <summary>
    ///     IP 地址
    /// </summary>
    [ObservableProperty] private string _ip = "192.168.0.1";

    /// <summary>
    ///     PLC 型号
    /// </summary>
    [ObservableProperty] private string _type = "S7-1500";

    /// <summary>
    ///     机架号
    /// </summary>
    [ObservableProperty] private int _rack;

    /// <summary>
    ///     插槽号
    /// </summary>
    [ObservableProperty] private int _slot = 1;

    /// <summary>
    ///     描述信息
    /// </summary>
    [ObservableProperty] private string _description = string.Empty;

    /// <summary>
    ///     是否启用
    /// </summary>
    [ObservableProperty] private bool _isEnabled = true;

    /// <summary>
    ///     状态文本
    /// </summary>
    [ObservableProperty] private string _statusText = "未连接";

    /// <summary>
    ///     节点列表
    /// </summary>
    [ObservableProperty] private ObservableCollection<S7PlcNode> _nodes = new();

    /// <summary>
    ///     是否正在测试连接
    /// </summary>
    [ObservableProperty] private bool _isTestingConnection;

    /// <summary>
    ///     是否正在保存
    /// </summary>
    [ObservableProperty] private bool _isSaving;

    #endregion

    #region 构造函数

    public S7PlcEditViewModel()
    {
        // 初始化默认值
        Key = $"plc_{DateTime.Now:yyyyMMddHHmmss}";
        Title = "新建PLC";
    }

    public S7PlcEditViewModel(S7PlcConfig config) : this()
    {
        LoadFromConfig(config);
    }

    #endregion

    #region 命令

    /// <summary>
    ///     保存命令
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (IsSaving) return;

        try
        {
            IsSaving = true;
            SaveCommand.NotifyCanExecuteChanged();

            // 验证数据
            var validationErrors = ValidateData();
            if (validationErrors.Any())
            {
                StatusText = $"验证失败: {string.Join(", ", validationErrors)}";
                return;
            }

            StatusText = "保存中...";

            // 模拟保存延迟
            await Task.Delay(1000);

            // 触发保存事件
            var config = ToS7PlcConfig();
            OnSaveRequested(config);

            StatusText = "保存成功";

            // 延迟清除状态文本
            await Task.Delay(2000);
            StatusText = "已保存";
        }
        catch (Exception ex)
        {
            StatusText = $"保存失败: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    ///     判断是否可以保存
    /// </summary>
    private bool CanSave()
    {
        return !IsSaving && !IsTestingConnection;
    }

    /// <summary>
    ///     测试连接命令
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanTestConnection))]
    private async Task TestConnectionAsync()
    {
        if (IsTestingConnection) return;

        try
        {
            IsTestingConnection = true;
            TestConnectionCommand.NotifyCanExecuteChanged();
            PingCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();

            StatusText = "测试连接中...";

            // 使用 Ping 测试网络连通性
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(Ip, 3000);

            if (reply.Status == IPStatus.Success)
            {
                StatusText = $"连接成功 (延迟: {reply.RoundtripTime}ms)";

                // 延迟清除状态文本
                await Task.Delay(3000);
                StatusText = "连接正常";
            }
            else
            {
                StatusText = $"连接失败: {reply.Status}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"连接测试失败: {ex.Message}";
        }
        finally
        {
            IsTestingConnection = false;
            TestConnectionCommand.NotifyCanExecuteChanged();
            PingCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    /// <summary>
    ///     判断是否可以测试连接
    /// </summary>
    private bool CanTestConnection()
    {
        return !IsTestingConnection && !IsSaving;
    }

    /// <summary>
    ///     Ping 命令（用于 XAML 中的 Ping 按钮）
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPing))]
    private async Task PingAsync()
    {
        await TestConnectionAsync();
    }

    /// <summary>
    ///     判断是否可以 Ping
    /// </summary>
    private bool CanPing()
    {
        return !IsTestingConnection && !IsSaving;
    }

    #endregion

    #region 数据转换方法

    /// <summary>
    ///     从 S7PlcConfig 加载数据
    /// </summary>
    /// <param name="config">S7 PLC 配置</param>
    public void LoadFromConfig(S7PlcConfig config)
    {
        if (config == null) return;

        Key = config.Key ?? string.Empty;
        Title = config.Name ?? string.Empty;
        Ip = config.Ip ?? "192.168.0.1";
        Type = config.Type ?? "S7-1500";
        Rack = config.Rack;
        Slot = config.Slot;
        Description = config.Description ?? string.Empty;
        IsEnabled = config.Enabled;
        StatusText = config.Enabled ? "已启用" : "已禁用";
        Nodes = config.Nodes;
    }

    /// <summary>
    ///     转换为 S7PlcConfig
    /// </summary>
    /// <returns>S7 PLC 配置</returns>
    public S7PlcConfig ToS7PlcConfig()
    {
        return new S7PlcConfig
        {
            Key = Key,
            Name = Title,
            Ip = Ip,
            Type = Type,
            Rack = (short)Rack,
            Slot = (short)Slot,
            Description = Description,
            Enabled = IsEnabled,
            Nodes = Nodes
        };
    }

    /// <summary>
    ///     验证数据
    /// </summary>
    /// <returns>验证错误列表</returns>
    public List<string> ValidateData()
    {
        var errors = new List<string>();

        // 验证 Key
        if (string.IsNullOrWhiteSpace(Key)) errors.Add("Key 不能为空");

        // 验证名称
        if (string.IsNullOrWhiteSpace(Title)) errors.Add("名称不能为空");

        // 验证 IP 地址
        if (string.IsNullOrWhiteSpace(Ip))
            errors.Add("IP 地址不能为空");
        else if (!IsValidIpAddress(Ip)) errors.Add("IP 地址格式不正确");

        // 验证机架号
        if (Rack < 0 || Rack > 7) errors.Add("机架号必须在 0-7 之间");

        // 验证插槽号
        if (Slot < 0 || Slot > 31) errors.Add("插槽号必须在 0-31 之间");

        return errors;
    }

    /// <summary>
    ///     验证 IP 地址格式
    /// </summary>
    /// <param name="ip">IP 地址</param>
    /// <returns>是否有效</returns>
    private bool IsValidIpAddress(string ip)
    {
        return IPAddress.TryParse(ip, out _);
    }

    #endregion

    #region 事件

    /// <summary>
    ///     保存请求事件
    /// </summary>
    public event EventHandler<S7PlcConfig>? SaveRequested;

    /// <summary>
    ///     数据变更事件
    /// </summary>
    public event EventHandler? DataChanged;

    /// <summary>
    ///     触发保存请求事件
    /// </summary>
    /// <param name="config">PLC 配置</param>
    protected virtual void OnSaveRequested(S7PlcConfig config)
    {
        SaveRequested?.Invoke(this, config);
    }

    /// <summary>
    ///     触发数据变更事件
    /// </summary>
    protected virtual void OnDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region 属性变更处理

    /// <summary>
    ///     Key 属性变更处理
    /// </summary>
    partial void OnKeyChanged(string value)
    {
        OnDataChanged();
    }

    /// <summary>
    ///     Title 属性变更处理
    /// </summary>
    partial void OnTitleChanged(string value)
    {
        OnDataChanged();
    }

    /// <summary>
    ///     IP 属性变更处理
    /// </summary>
    partial void OnIpChanged(string value)
    {
        OnDataChanged();
        // 清除之前的状态文本
        if (StatusText.Contains("连接")) StatusText = "未连接";
    }

    /// <summary>
    ///     Type 属性变更处理
    /// </summary>
    partial void OnTypeChanged(string value)
    {
        OnDataChanged();
    }

    /// <summary>
    ///     Rack 属性变更处理
    /// </summary>
    partial void OnRackChanged(int value)
    {
        OnDataChanged();
    }

    /// <summary>
    ///     Slot 属性变更处理
    /// </summary>
    partial void OnSlotChanged(int value)
    {
        OnDataChanged();
    }

    /// <summary>
    ///     IsEnabled 属性变更处理
    /// </summary>
    partial void OnIsEnabledChanged(bool value)
    {
        StatusText = value ? "已启用" : "已禁用";
        OnDataChanged();
    }

    #endregion

    #region 静态工厂方法

    /// <summary>
    ///     从 S7PlcConfig 创建 ViewModel
    /// </summary>
    /// <param name="config">S7 PLC 配置</param>
    /// <returns>ViewModel 实例</returns>
    public static S7PlcEditViewModel FromConfig(S7PlcConfig config)
    {
        return new S7PlcEditViewModel(config);
    }

    /// <summary>
    ///     创建新的 ViewModel 实例
    /// </summary>
    /// <returns>新的 ViewModel 实例</returns>
    public static S7PlcEditViewModel CreateNew()
    {
        return new S7PlcEditViewModel();
    }

    #endregion
}