using Core.Interfaces;
using SqlSugar;

namespace Core.Models;

[SugarIndex("IX_SerialNo", nameof(SerialNo), OrderByType.Asc)]
public class RecordBase
{
	/// <summary>
	/// 表示数据库表的主键ID，唯一标识每条记录。
	/// 该字段被标记为主键且为自增列。
	/// </summary>
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "ID", IsOnlyIgnoreInsert = true)]
	public int Id { get; set; }

	/// <summary>
	/// 序列号
	/// </summary>
	[SugarColumn(ColumnDescription = "序列号", IsNullable = true)]
	public string? SerialNo { get; set; }
	
	/// <summary>
	/// 对应的工序ID, 可以启用工序的记录，也可以直接进行记录
	/// </summary>
	[SugarColumn(ColumnDescription = "工序ID", IsNullable = true)]
	public int ProcessId { get; set; }

	/// <summary>
	/// 机器名称，比如R1
	/// </summary>
	[SugarColumn(ColumnDescription = "机器名称", IsNullable = true)]
	public string MachineName { get; set; } = string.Empty;

	/// <summary>
	/// 工序名称，比如Op10
	/// </summary>
	[SugarColumn(ColumnDescription = "工序名称", IsNullable = true)]
	public string ProcessName { get; set; } = string.Empty;	
	
	/// <summary>
	/// 工作站名称，比如主线体、分总成站、单机点焊站等
	/// </summary>
	[SugarColumn(ColumnDescription = "工作站名称", IsNullable = true)]
	public string StationName { get; set; }= string.Empty;	
	
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