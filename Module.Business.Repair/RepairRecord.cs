using Core.Models;
using SqlSugar;

namespace Module.Business.Repair;

/// <summary>
///  返修记录
/// </summary>
[SugarTable(TableDescription = "返修记录")]
public class RepairRecord: RecordBase
{
    /// <summary>
    /// 零件码
    /// </summary>
    [SugarColumn(ColumnDescription = "零件码", Length = 50)]
    public string? PartCode { get; set; }

    /// <summary>
    /// 下料时间
    /// </summary>
    [SugarColumn(ColumnDescription = "下料时间")]
    public DateTime UnloadTime { get; set; }

    /// <summary>
    /// 重新上料时间
    /// </summary>
    [SugarColumn(ColumnDescription = "重新上料时间", IsNullable = true)]
    public DateTime? LoadTime { get; set; }
}