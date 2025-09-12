using Core.Models;

namespace Module.Business.SG141;

/// <summary>
/// SG141的模块配置
/// </summary>
public class SG141Settings: ConfigBase
{
    // 1. 生产趋势模块 - 每日计划产量
    public int DailyPlanCount { get; set; } = 0;
    
    // 计划生产速度
    public int PlanSpeed { get; set; } = 0;

    /// <summary>
    /// 设备列表定义
    /// </summary>
    public List<DeviceSetting> DeviceSettings { get; set; } = new();
}

/// <summary>
/// 用来配置设备及对应节点(PLC设备状态节点)
/// </summary>
public class DeviceSetting : ConfigBase
{
    /// <summary>
    ///  设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;
    
    /// <summary>
    /// 设备对应PLC的key
    /// </summary>
    public string DevicePlcKey { get; set; }

    /// <summary>
    /// 设备状态节点
    /// </summary>
    public string PlcNodeKey { get; set; }

    public bool IsActive { get; set; } = true;
}