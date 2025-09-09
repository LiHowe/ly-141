using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UI.Controls;

public partial class DeviceConnectionControl : UserControl
{
    public DeviceConnectionControl()
    {
        InitializeComponent();
    }
}

public partial class DeviceConnectionViewModel : ObservableObject
{
    /// <summary>
    ///     设备图标
    /// </summary>
    [ObservableProperty] private string _icon = "\ue672";

    /// <summary>
    ///     设备IP
    /// </summary>
    [ObservableProperty] private string _ip = string.Empty;

    /// <summary>
    ///     设备名称
    /// </summary>
    [ObservableProperty] private string _name = string.Empty;

    /// <summary>
    ///     设备状态
    /// </summary>
    [ObservableProperty] private DeviceConnectionStatus _status = DeviceConnectionStatus.Disconnect;

    /// <summary>
    ///     状态颜色
    /// </summary>
    [ObservableProperty] private Brush _statusBrush = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));

    [ObservableProperty] private string _tipContent = string.Empty;

    public DeviceConnectionViewModel()
    {
        UpdateStatusBrush();
    }

    // 定义一个事件，供外界订阅
    public event EventHandler? Clicked;

    partial void OnStatusChanged(DeviceConnectionStatus oldValue, DeviceConnectionStatus newValue)
    {
        UpdateStatusBrush();
    }

    [RelayCommand]
    private void HandleClick()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     更新状态画刷
    /// </summary>
    private void UpdateStatusBrush()
    {
        StatusBrush = Status switch
        {
            DeviceConnectionStatus.Disconnect => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)), // 深暗色绿色
            DeviceConnectionStatus.Normal => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)), // 亮绿色
            DeviceConnectionStatus.Error => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)), // 红色
            _ => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
        };
        Icon = Status switch
        {
            DeviceConnectionStatus.Disconnect => "\ue673",
            DeviceConnectionStatus.Normal => "\ue672",
            DeviceConnectionStatus.Error => "\ue601",
            _ => "\ue672"
        };
        TipContent = Status switch
        {
            DeviceConnectionStatus.Disconnect => "未连接",
            DeviceConnectionStatus.Normal => "连接正常",
            DeviceConnectionStatus.Error => "连接错误",
            _ => "未知"
        };
    }

    partial void OnTipContentChanged(string value)
    {
        OnPropertyChanged(nameof(TipContent));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum DeviceConnectionStatus
{
    [Description("正常")] Normal,
    [Description("离线")] Disconnect,
    [Description("连接错误")] Error
}