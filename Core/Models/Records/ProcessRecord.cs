using SqlSugar;

namespace Core.Models.Records;

/// <summary>
/// 工序记录
/// 产品 -> 工序 -> 点焊、凸焊等详细记录
/// </summary>
[SugarTable(TableDescription = "工序记录表")]
public class ProcessRecord
{
    /// <summary>
    /// 表示数据库表的主键ID，唯一标识每条记录。
    /// 该字段被标记为主键且为自增列。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "ID", IsOnlyIgnoreInsert = true)]
    public int Id { get; set; }
    
    /// <summary>
    /// 工序名称
    /// </summary>
    [SugarColumn(ColumnDescription = "工序名称")]
    public string ProcessName { get; set; } = string.Empty;
    
    /// <summary>
    /// 工序类型，比如点焊
    /// </summary>
    [SugarColumn(ColumnDescription = "工序类型", IsNullable = true)]
    public string ProcessType { get; set; } = string.Empty;
    
    /// <summary>
    /// 记录创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsOnlyIgnoreUpdate = true, InsertServerTime = true, IsNullable = true)]
    public DateTime CreateTime { get; set; } = DateTime.Now;
	
    /// <summary>
    ///  记录更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间", IsOnlyIgnoreInsert = true, UpdateServerTime = true, IsNullable = true)]
    public DateTime? UpdateTime { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", IsNullable = true)]
    public string? Remark { get; set; }
    
    
}