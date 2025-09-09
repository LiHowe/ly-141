using SqlSugar;

namespace Core.Models.Records;

[SugarTable(TableDescription = "涂胶检测记录表")]
public class ImlightRecord: BizRecordBase
{
	/// <summary>
	/// 工程编码
	/// </summary>
	[SugarColumn(ColumnDescription = "工程编码", IsNullable = true)]
	public string? ProjectCode { get; set; }

	/// <summary>
	/// 工件名称
	/// </summary>
	[SugarColumn(ColumnDescription = "工件名称", IsNullable = true)]
	public string? WorkpieceName { get; set; }

	/// <summary>
	/// 轨迹名称
	/// </summary>
	[SugarColumn(ColumnDescription = "轨迹名称", IsNullable = true)]
	public string? TrackName { get; set; }

	/// <summary>
	/// 模板名称
	/// </summary>
	[SugarColumn(ColumnDescription = "模板名称", IsNullable = true)]
	public string? TemplateName { get; set; }

	/// <summary>
	/// 分段名称
	/// </summary>
	[SugarColumn(ColumnDescription = "分段名称", IsNullable = true)]
	public string? SegmentName { get; set; }

	/// <summary>
	/// VIN码
	/// </summary>
	[SugarColumn(ColumnDescription = "VIN码", IsNullable = true)]
	public string? Vin { get; set; }

	/// <summary>
	/// 段号
	/// </summary>
	[SugarColumn(ColumnDescription = "段号")]
	public int SegmentNumber { get; set; }

	/// <summary>
	/// 起始帧
	/// </summary>
	[SugarColumn(ColumnDescription = "起始帧")]
	public int StartFrame { get; set; }

	/// <summary>
	/// 结束帧
	/// </summary>
	[SugarColumn(ColumnDescription = "结束帧")]
	public int EndFrame { get; set; }

	/// <summary>
	/// 标准宽度
	/// </summary>
	[SugarColumn(ColumnDescription = "标准宽度")]
	public double StandardWidth { get; set; }

	/// <summary>
	/// 宽度上限
	/// </summary>
	[SugarColumn(ColumnDescription = "宽度上限")]
	public double WidthUpperLimit { get; set; }

	/// <summary>
	/// 宽度下限
	/// </summary>
	[SugarColumn(ColumnDescription = "宽度下限")]
	public double WidthLowerLimit { get; set; }

	/// <summary>
	/// 平均宽度
	/// </summary>
	[SugarColumn(ColumnDescription = "平均宽度")]
	public double AverageWidth { get; set; }

	/// <summary>
	/// 最小宽度
	/// </summary>
	[SugarColumn(ColumnDescription = "最小宽度")]
	public double MinWidth { get; set; }

	/// <summary>
	/// 最大宽度
	/// </summary>
	[SugarColumn(ColumnDescription = "最大宽度")]
	public double MaxWidth { get; set; }

	/// <summary>
	/// 标准高度
	/// </summary>
	[SugarColumn(ColumnDescription = "标准高度")]
	public double StandardHeight { get; set; }

	/// <summary>
	/// 高度上限
	/// </summary>
	[SugarColumn(ColumnDescription = "高度上限")]
	public double HeightUpperLimit { get; set; }

	/// <summary>
	/// 高度下限
	/// </summary>
	[SugarColumn(ColumnDescription = "高度下限")]
	public double HeightLowerLimit { get; set; }

	/// <summary>
	/// 平均高度
	/// </summary>
	[SugarColumn(ColumnDescription = "平均高度")]
	public double AverageHeight { get; set; }

	/// <summary>
	/// 最小高度
	/// </summary>
	[SugarColumn(ColumnDescription = "最小高度")]
	public double MinHeight { get; set; }

	/// <summary>
	/// 最大高度
	/// </summary>
	[SugarColumn(ColumnDescription = "最大高度")]
	public double MaxHeight { get; set; }

	/// <summary>
	/// 标准最大偏移
	/// </summary>
	[SugarColumn(ColumnDescription = "标准最大偏移")]
	public double StandardMaxOffset { get; set; }

	/// <summary>
	/// 平均偏移
	/// </summary>
	[SugarColumn(ColumnDescription = "平均偏移")]
	public double AverageOffset { get; set; }

	/// <summary>
	/// 最小偏移
	/// </summary>
	[SugarColumn(ColumnDescription = "最小偏移")]
	public double MinOffset { get; set; }

	/// <summary>
	/// 最大偏移
	/// </summary>
	[SugarColumn(ColumnDescription = "最大偏移")]
	public double MaxOffset { get; set; }

	/// <summary>
	/// 标准倾角
	/// </summary>
	[SugarColumn(ColumnDescription = "标准倾角")]
	public double StandardAngle { get; set; }

	/// <summary>
	/// 倾角上限
	/// </summary>
	[SugarColumn(ColumnDescription = "倾角上限")]
	public double AngleUpperLimit { get; set; }

	/// <summary>
	/// 倾角下限
	/// </summary>
	[SugarColumn(ColumnDescription = "倾角下限")]
	public double AngleLowerLimit { get; set; }

	/// <summary>
	/// 平均倾角
	/// </summary>
	[SugarColumn(ColumnDescription = "平均倾角")]
	public double AverageAngle { get; set; }

	/// <summary>
	/// 最小倾角
	/// </summary>
	[SugarColumn(ColumnDescription = "最小倾角")]
	public double MinAngle { get; set; }

	/// <summary>
	/// 最大倾角
	/// </summary>
	[SugarColumn(ColumnDescription = "最大倾角")]
	public double MaxAngle { get; set; }

	/// <summary>
	/// 标准面积
	/// </summary>
	[SugarColumn(ColumnDescription = "标准面积")]
	public double StandardArea { get; set; }

	/// <summary>
	/// 面积上限
	/// </summary>
	[SugarColumn(ColumnDescription = "面积上限")]
	public double AreaUpperLimit { get; set; }

	/// <summary>
	/// 面积下限
	/// </summary>
	[SugarColumn(ColumnDescription = "面积下限")]
	public double AreaLowerLimit { get; set; }

	/// <summary>
	/// 平均面积
	/// </summary>
	[SugarColumn(ColumnDescription = "平均面积")]
	public double AverageArea { get; set; }

	/// <summary>
	/// 最小面积
	/// </summary>
	[SugarColumn(ColumnDescription = "最小面积")]
	public double MinArea { get; set; }

	/// <summary>
	/// 最大面积
	/// </summary>
	[SugarColumn(ColumnDescription = "最大面积")]
	public double MaxArea { get; set; }

	/// <summary>
	/// 分段结果
	/// </summary>
	[SugarColumn(ColumnDescription = "分段结果")]
	public string SegmentResult { get; set; } = "OK";
}