using System.Windows.Media;
using Connection.S7;
using UI.ViewModels;

namespace UI.Converters;

/// <summary>
///     用来转换S7PlcConfig 与 S7PlcSettingViewModel
/// </summary>
public static class S7PlcConfigConverter
{
    // 将 S7PlcConfig 转换为 S7PlcSettingViewModel
    public static S7PlcViewModel ToViewModel(S7PlcConfig config)
    {
        if (config == null) return null;

        var plc = config.GetPlc();
        try
        {
            if (!plc.IsConnected && config.Enabled)
                plc.ConnectAsync();
        }
        catch (Exception)
        {
            // ignored
        }

        return new S7PlcViewModel
        {
            DeviceId = config.Key,
            DeviceName = config.Name,
            DeviceType = "Plc设备", // 根据需要调整
            DeviceTypeName = config.Type,
            IpAddress = config.Ip,
            RackNumber = config.Rack,
            SlotNumber = config.Slot,
            IsEnabled = config.Enabled,
            NodeCount = config.Nodes?.Count ?? 0,
            Status = plc.IsConnected ? plc.IsRunning ? "运行中" : "停止" : "未连接",
            StatusColor = plc.IsRunning ? Brushes.Green : Brushes.Red,
            Description = config.Description,
            Nodes = config.Nodes
        };
    }

    // 将 S7PlcSettingViewModel 转换为 S7PlcConfig
    public static S7PlcConfig ToConfig(S7PlcViewModel viewModel)
    {
        if (viewModel == null) return null;

        return new S7PlcConfig
        {
            Key = viewModel.DeviceId,
            Name = viewModel.DeviceName,
            Type = viewModel.DeviceTypeName,
            Ip = viewModel.IpAddress,
            Rack = (short)viewModel.RackNumber,
            Slot = (short)viewModel.SlotNumber,
            Enabled = viewModel.IsEnabled,
            Description = null, // 如果 ViewModel 中有 Description 属性，可以映射
            Nodes = viewModel.Nodes // 根据需要处理 Nodes
        };
    }
}