using Core.Interfaces;

namespace Module.Business.SG141.Repositories;

/// <summary>
///   设备故障查询
/// </summary>
public class DeviceFaultQuery: IPagedQuery
{
    public int PageIndex { get; set; } = 1;
    
    public int PageSize { get; set; } = 10;
    
    /// <summary>
    /// 设备名称
    /// </summary>
    public string? DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 故障类型
    /// </summary>
    public string? FaultType { get; set; } = string.Empty;
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public string? StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 结束时间
    /// </summary>
    public string? EndTime { get; set; } = string.Empty;
}

public class DeviceFaultStatistic
{
    public string DeviceName { get; set; } = string.Empty;
    
    public int Count { get; set; }
}