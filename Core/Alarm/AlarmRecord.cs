using System.ComponentModel;
using SqlSugar;

namespace Core.Alarm;


[SugarTable("AlarmRecords",TableDescription = "报警记录")]
public class AlarmRecord
{
	/// <summary>
	/// 表示数据库表的主键ID，唯一标识每条记录。
	/// 该字段被标记为主键且为自增列。
	/// </summary>
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "ID", IsOnlyIgnoreInsert = true)]
	public int Id { get; set; }


	/// <summary>
	/// 表示报警的级别，使用 AlarmLevel 枚举定义。
	/// 数据库字段类型为 tinyint，用于存储小型整数值。
	/// </summary>
	[SugarColumn(ColumnDescription = "报警级别")]
	public AlarmLevel Level { get; set; } = AlarmLevel.Info;

	/// <summary>
	/// 日志等级对应的颜色
	/// </summary>
	[SugarColumn(IsIgnore = true)]
	public string LevelColor => Level switch
	{
		AlarmLevel.Info => "#6C757D",
		AlarmLevel.Warning => "#ffc516",
		AlarmLevel.Error => "#FF5723",
		AlarmLevel.Critical => "#ff1100",
		_ => "#6C757D"
	};
	
	/// <summary>
	/// 表示触发报警的模块名称。
	/// 数据库字段长度限制为 50 个字符。
	/// </summary>
	[SugarColumn(Length = 50, ColumnDescription = "模块名称")]
	public string? Module { get; set; }

	/// <summary>
	/// 表示报警的类别。
	/// 数据库字段长度限制为 50 个字符。
	/// </summary>
	[SugarColumn(ColumnDescription = "报警类别")]
	public AlarmCategory Category { get; set; } = AlarmCategory.System;

	/// <summary>
	/// 表示报警的详细信息内容。
	/// 数据库字段类型为 nvarchar(max)，支持存储大文本。
	/// </summary>
	[SugarColumn(ColumnDescription = "报警详情")]
	public string Message { get; set; } = string.Empty;

	/// <summary>
	/// 表示报警触发的时间，默认值为当前时间。
	/// 数据库字段类型为 datetime。
	/// </summary>
	[SugarColumn(ColumnDescription = "报警时间")]
	public DateTime TriggerTime { get; set; } = DateTime.Now;

	/// <summary>
	/// 表示报警被解决的时间，允许为空。
	/// 数据库字段类型为 datetime，且可为空。
	/// </summary>
	[SugarColumn(ColumnDescription = "解决时间", IsNullable = true)]
	public DateTime? ResolvedTime { get; set; }

	/// <summary>
	/// 表示解决报警的操作者姓名，允许为空。
	/// 数据库字段长度限制为 100 个字符。
	/// </summary>
	[SugarColumn(Length = 100, ColumnDescription = "解决者", IsNullable = true)]
	public string? ResolvedBy { get; set; }

	/// <summary>
	/// 表示关于报警的额外说明或注释，允许为空。
	/// 数据库字段长度限制为 500 个字符。
	/// </summary>
	[SugarColumn(Length = 500, ColumnDescription = "备注", IsNullable = true)]
	public string? Remarks { get; set; }

	/// <summary>
	/// 表示报警的当前状态，使用 AlarmStatus 枚举定义，默认值为 New。
	/// 数据库字段类型为 tinyint。
	/// </summary>
	[SugarColumn(ColumnDescription = "报警状态")]
	public AlarmStatus Status { get; set; } = AlarmStatus.New;

	/// <summary>
	/// 表示报警触发时的相关数据快照。
	/// 数据库字段类型为 nvarchar(max)，支持存储大文本。
	/// </summary>
	[SugarColumn(ColumnDescription = "报警数据快照", IsNullable = true)]
	public string? DataSnapshot { get; set; }
}

// 报警等级枚举（添加中文描述）
public enum AlarmLevel
{
	[Description("一般信息")]
	Info = 1,
	[Description("警告")]
	Warning = 2,
	[Description("错误")]
	Error = 3,
	[Description("严重故障")]
	Critical = 4
}

// 报警状态枚举（添加中文描述）
public enum AlarmStatus
{
	[Description("新报警")]
	New = 1,
	[Description("已解决")]
	Resolved = 2
}

// 报警类别枚举（添加中文描述）
public enum AlarmCategory
{
	// 比如无网络、磁盘空间不足等
	[Description("系统相关")]
	System = 1,
	// 比如接收到的信号表示硬件故障
	[Description("硬件故障")]
	Hardware = 2,
	// 各种连接断开
	[Description("网络问题")]
	Network = 3,
	// 加工质量NG
	[Description("质量相关")]
	Quality = 4,
	// 某些方法执行时间过长
	[Description("性能问题")]
	Performance = 5,
	// 程序出错
	[Description("应用程序错误")]
	Application = 6,
	// 用户误操作或修改了某些关键配置
	[Description("用户操作相关")]
	UserAction = 7
}