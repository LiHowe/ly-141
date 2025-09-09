using CommunityToolkit.Mvvm.ComponentModel;
using SqlSugar;

namespace Core.Models.Records;

/// <summary>
/// 产品记录类
/// </summary>
[SugarTable(TableDescription = "产品记录表")]
public class ProductRecord: RecordBase
{
    /// <summary>
    /// 序列号
    /// </summary>
    [SugarColumn(ColumnDescription = "序列号")]
    public string SerialNo { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    [SugarColumn(ColumnDescription = "产品名称", IsNullable = true)]
    public string? ProductName { get; set; }

    /// <summary>
    /// 产品型号
    /// </summary>
    [SugarColumn(ColumnDescription = "产品型号", IsNullable = true)]
    public string? ProductModel { get; set; }

    /// <summary>
    /// 工序号
    /// </summary>
    [SugarColumn(ColumnDescription = "工序号", IsNullable = true)]
    public string? ProcessNo { get; set; }

    /// <summary>
    /// 批次号
    /// </summary>
    [SugarColumn(ColumnDescription = "批次号", IsNullable = true)]
    public string? BatchNo { get; set; }

   
    /// <summary>
    /// 质量状态
    /// </summary>
    [SugarColumn(ColumnDescription = "质量状态", IsNullable = true)]
    public string? Quality { get; set; } = "OK";

    /// <summary>
    /// 是否返修过
    /// </summary>
    [SugarColumn(ColumnDescription = "是否返修", IsNullable = true)]
    public bool? IsRepaired { get; set; } = false;

    /// <summary>
    ///  记录更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "返修时间", IsNullable = true)]
    public DateTime? RepairTime { get; set; } = null;
    
}