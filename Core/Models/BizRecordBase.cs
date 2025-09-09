using SqlSugar;

namespace Core.Models;

/// <summary>
/// 业务记录基类
/// </summary>
public class BizRecordBase: RecordBase
{
    /// <summary>
    /// 设备名称, 记录点焊记录产生的设备名称
    /// </summary>
    [SugarColumn(ColumnDescription = "设备名称", IsNullable = true)]
    public string? DeviceName { get; set; } = string.Empty;
	
    /// <summary>
    /// 工位名称
    /// </summary>
    [SugarColumn(ColumnDescription = "工位名称", IsNullable = true)]
    public string? StationName { get; set; } = string.Empty;
	
    /// <summary>
    /// 工序ID - 为后期零件工序功能预留
    /// </summary>
    [SugarColumn(ColumnDescription = "工序ID", IsNullable = true)]
    public int? ProcessId { get; set; }
    
    [SugarColumn(ColumnDescription = "设备ID", IsNullable = true)]
    public int? DeviceId { get; set; }
}