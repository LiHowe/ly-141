using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Connection.S7;
using UI.Controls;
using UI.Converters;
using MessageBox = UI.Controls.MessageBox;

namespace UI.ViewModels;

/// <summary>
///     S7 PLC 视图模型
///     用于显示和管理单个 S7 PLC 设备的信息和操作
/// </summary>
public partial class S7PlcViewModel : ObservableObject
{
    #region 构造函数

    /// <summary>
    ///     初始化 S7PlcViewModel 的新实例
    /// </summary>
    public S7PlcViewModel()
    {
        // 监听节点集合变更事件
        Nodes.CollectionChanged += (s, e) =>
        {
            NodeCount = Nodes.Count;
            OnSettingsChanged();
        };
    }

    #endregion

    #region 基本属性

    /// <summary>
    ///     设备名称
    /// </summary>
    [ObservableProperty] private string _deviceName = "plc_1";

    /// <summary>
    ///     设备标识符
    /// </summary>
    [ObservableProperty] private string _deviceId = "plc1";

    /// <summary>
    ///     设备类型描述
    /// </summary>
    [ObservableProperty] private string _deviceType = "西门子PLC";

    /// <summary>
    ///     设备型号名称
    /// </summary>
    [ObservableProperty] private string _deviceTypeName = "S7-1500";

    /// <summary>
    ///     设备描述信息
    /// </summary>
    [ObservableProperty] private string _description = "";

    /// <summary>
    ///     IP 地址
    /// </summary>
    [ObservableProperty] private string _ipAddress = "192.168.0.1";

    /// <summary>
    ///     机架号 (0-7)
    /// </summary>
    [ObservableProperty] private int _rackNumber;

    /// <summary>
    ///     插槽号 (0-31)
    /// </summary>
    [ObservableProperty] private int _slotNumber = 1;

    /// <summary>
    ///     是否启用此 PLC
    /// </summary>
    [ObservableProperty] private bool _isEnabled = true;

    #endregion

    #region 状态属性

    /// <summary>
    ///     当前状态文本
    /// </summary>
    [ObservableProperty] private string _status = "未测试";

    /// <summary>
    ///     状态颜色画刷
    /// </summary>
    [ObservableProperty] private Brush _statusColor = Brushes.Gray;

    #endregion

    #region 节点相关属性

    /// <summary>
    ///     节点数量
    /// </summary>
    [ObservableProperty] private int _nodeCount;

    /// <summary>
    ///     PLC 节点集合
    /// </summary>
    [ObservableProperty] private ObservableCollection<S7PlcNode> _nodes = new();

    #endregion

    #region 命令

    /// <summary>
    ///     测试连接命令
    ///     测试与 PLC 的网络连接状态
    /// </summary>
    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        try
        {
            Status = "连接中...";
            StatusColor = Brushes.Orange;

            // 使用 Ping 测试网络连通性
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(IpAddress, 3000);

            var plc = ToS7Config().GetPlc();
            await plc.ConnectAsync();

            if (reply.Status == IPStatus.Success)
            {
                Status = $"连接成功\n(延迟: {reply.RoundtripTime}ms)";
                StatusColor = Brushes.Green;
            }
            else
            {
                MessageBox.Error("连接失败", details: "Ping失败");
                Status = "连接失败";
                StatusColor = Brushes.Red;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Error($"连接失败:{ex.Message}");
            Status = "连接失败";
            StatusColor = Brushes.Red;
        }
    }

    /// <summary>
    ///     编辑设备命令
    ///     打开设备编辑对话框进行配置修改
    /// </summary>
    [RelayCommand]
    private void EditDevice()
    {
        try
        {
            // 创建编辑窗口
            var editWindow = new ShellWindow("编辑PLC配置", "⚙️");

            editWindow.Width = 300;
            editWindow.Height = 300;

            // 创建编辑 ViewModel 并加载当前配置
            var editViewModel = S7PlcEditViewModel.FromConfig(ToS7Config());

            // 订阅保存事件
            editViewModel.SaveRequested += (sender, config) =>
            {
                // 更新当前 ViewModel 的属性
                UpdateFromConfig(config);

                // 触发设置变更事件
                OnSettingsChanged();

                // 关闭编辑窗口
                editWindow.Close();
            };

            // 创建编辑控件并设置到窗口
            var editControl = new S7PlcEditControl(editViewModel);
            editWindow.SetContent(editControl);

            // 显示编辑窗口
            editWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            // 显示错误消息
            MessageBox.Error($"打开编辑窗口失败: {ex.Message}");
        }
    }

    #endregion

    #region 其他命令

    /// <summary>
    ///     移除PLC设备命令
    ///     从配置中移除当前 PLC 设备
    /// </summary>
    [RelayCommand]
    private void RemoveDevice()
    {
        try
        {
            // 显示确认对话框
            var result = MessageBox.Question(
                $"确定要删除 PLC '{DeviceName}' 吗？\n此操作不可撤销。");

            if (result == MessageBoxResult.Yes)
                // 触发删除事件，由父级处理实际删除逻辑
                OnDeviceRemoveRequested();
        }
        catch (Exception ex)
        {
            MessageBox.Error($"删除设备失败: {ex.Message}");
        }
    }

    /// <summary>
    ///     编辑节点命令
    ///     打开节点编辑器对话框
    /// </summary>
    [RelayCommand]
    private void EditNodes()
    {
        try
        {
            // 创建节点编辑窗口
            var nodeEditWindow = new ShellWindow("编辑节点", "⚙️");

            // 创建节点编辑 ViewModel
            var nodeEditViewModel = new S7NodeEditorViewModel
            {
                Nodes = new ObservableCollection<S7NodeViewModel>(Nodes.Select(S7NodeConverter.ToViewModel))
            };

            // 创建节点编辑控件
            var nodeEditControl = new S7NodeEditorControl(nodeEditViewModel);
            nodeEditWindow.SetContent(nodeEditControl);

            // 显示对话框
            nodeEditWindow.ShowDialog();

            // 如果用户确认保存，更新节点数据
            Nodes = new ObservableCollection<S7PlcNode>(nodeEditViewModel.Nodes.Select(S7NodeConverter.ToNode));
            NodeCount = Nodes.Count;
            OnSettingsChanged();
        }
        catch (Exception ex)
        {
            MessageBox.Error($"打开节点编辑器失败: {ex.Message}");
        }
    }

    #endregion

    #region 数据转换方法

    /// <summary>
    ///     将当前 ViewModel 转换为 S7PlcConfig 配置对象
    /// </summary>
    /// <returns>S7 PLC 配置对象</returns>
    public S7PlcConfig ToS7Config()
    {
        return new S7PlcConfig
        {
            Key = DeviceId,
            Name = DeviceName,
            Type = DeviceTypeName,
            Ip = IpAddress,
            Rack = (short)RackNumber,
            Slot = (short)SlotNumber,
            Enabled = IsEnabled,
            Description = Description,
            Nodes = Nodes
        };
    }

    /// <summary>
    ///     从 S7PlcConfig 配置对象创建 ViewModel 实例
    /// </summary>
    /// <param name="config">S7 PLC 配置对象</param>
    /// <returns>S7PlcViewModel 实例</returns>
    public static S7PlcViewModel FromS7Config(S7PlcConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        return new S7PlcViewModel
        {
            DeviceId = config.Key ?? string.Empty,
            DeviceName = config.Name ?? string.Empty,
            DeviceTypeName = config.Type ?? "S7-1500",
            IpAddress = config.Ip ?? "192.168.0.1",
            RackNumber = config.Rack,
            SlotNumber = config.Slot,
            IsEnabled = config.Enabled,
            Description = config.Description ?? string.Empty,
            Nodes = config.Nodes ?? new ObservableCollection<S7PlcNode>()
        };
    }

    /// <summary>
    ///     从配置对象更新当前 ViewModel 的属性
    /// </summary>
    /// <param name="config">S7 PLC 配置对象</param>
    public void UpdateFromConfig(S7PlcConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        DeviceId = config.Key ?? string.Empty;
        DeviceName = config.Name ?? string.Empty;
        DeviceTypeName = config.Type ?? "S7-1500";
        IpAddress = config.Ip ?? "192.168.0.1";
        RackNumber = config.Rack;
        SlotNumber = config.Slot;
        IsEnabled = config.Enabled;
        Description = config.Description ?? string.Empty;

        // 更新节点集合
        if (config.Nodes != null) Nodes = new ObservableCollection<S7PlcNode>(config.Nodes);
    }

    #endregion

    #region 属性变更处理

    /// <summary>
    ///     节点集合变更时的处理方法
    /// </summary>
    /// <param name="value">新的节点集合</param>
    partial void OnNodesChanged(ObservableCollection<S7PlcNode> value)
    {
        OnSettingsChanged();
    }

    partial void OnIsEnabledChanged(bool value)
    {
        OnSettingsChanged();
    }

    #endregion

    #region 事件

    /// <summary>
    ///     设置变更事件
    ///     当 PLC 配置发生变更时触发
    /// </summary>
    public event EventHandler? SettingsChanged;

    /// <summary>
    ///     设备删除请求事件
    ///     当用户请求删除此 PLC 设备时触发
    /// </summary>
    public event EventHandler? DeviceRemoveRequested;

    /// <summary>
    ///     触发设置变更事件
    /// </summary>
    protected virtual void OnSettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     触发设备删除请求事件
    /// </summary>
    protected virtual void OnDeviceRemoveRequested()
    {
        DeviceRemoveRequested?.Invoke(this, EventArgs.Empty);
    }

    #endregion
}