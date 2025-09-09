using Core.Models;
using SqlSugar;

namespace Module.Business.Rivet;

/// <summary>
/// 拉铆记录
/// </summary>
[SugarTable(TableDescription = "拉铆记录")]
public class RivetRecord: RecordBase
{
    /// <summary>
    ///  拉力，单位：千牛（N）
    /// </summary>
    [SugarColumn(ColumnDescription = "拉力")]
    public double Force { get; set; }

    /// <summary>
    /// 位移/行程，单位：毫米（mm）
    /// </summary>
    [SugarColumn(ColumnDescription = "位移")]
    public double Displacement { get; set; }
    
    /// <summary>
    /// 铆钉索引
    /// </summary>
    [SugarColumn(ColumnDescription = "铆钉索引", IsNullable = true)]
    public int RivetIndex { get; set; }
    
    /// <summary>
    ///  铆钉名称
    /// </summary>
    [SugarColumn(ColumnDescription = "铆钉名称", IsNullable = true)]
    public string RivetName { get; set; } = string.Empty;
    
}