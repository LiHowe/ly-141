using SqlSugar;

namespace Core.Models.Records;

[SugarTable(TableDescription = "点焊记录表")]
public class SpotWeldRecord : BizRecordBase
{
	/// <summary>
	/// 焊接电压（伏特）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电压")]
	public double Voltage { get; set; }

	/// <summary>
	/// 焊接时间（周期或毫秒，视具体定义）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接时间")]
	public int WeldTime { get; set; }

	/// <summary>
	/// 第一阶段焊接电流（安培）
	/// </summary>
	[SugarColumn(ColumnDescription = "电流1")]
	public double Current1 { get; set; }

	/// <summary>
	/// 第二阶段焊接电流（安培）
	/// </summary>
	[SugarColumn(ColumnDescription = "电流2")]
	public double Current2 { get; set; }

	/// <summary>
	/// 第三阶段焊接电流（安培）
	/// </summary>
	[SugarColumn(ColumnDescription = "电流3")]
	public double Current3 { get; set; }

	/// <summary>
	/// 焊接点名称
	/// </summary>
	[SugarColumn(ColumnDescription = "焊点名称", IsNullable = true)]
	public string? SpotName { get; set; }

	/// <summary>
	/// 焊接点索引
	/// </summary>
	[SugarColumn(ColumnDescription = "焊点序号")]
	public int SpotIndex { get; set; }

	/// <summary>
	/// 电阻值（欧姆）
	/// </summary>
	[SugarColumn(ColumnDescription = "电阻值")]
	public double Resistance { get; set; }
	
	/// <summary>
	/// 电极力（牛顿）
	/// </summary>
	[SugarColumn(ColumnDescription = "电极力")]
	public double ElectrodeForce { get; set; }

	/// <summary>
	/// 材料类型
	/// </summary>
	[SugarColumn(ColumnDescription = "材料类型", IsNullable = true)]
	public string? MaterialType { get; set; }

	/// <summary>
	/// 材料厚度（毫米）
	/// </summary>
	[SugarColumn(ColumnDescription = "材料厚度")]
	public double Thickness { get; set; }

	/// <summary>
	/// 电极直径（毫米） - 影响焊点尺寸
	/// </summary>
	[SugarColumn(ColumnDescription = "电极直径")]
	public double ElectrodeDiameter { get; set; }

	/// <summary>
	/// 冷却时间（毫秒） - 防止过热
	/// </summary>
	[SugarColumn(ColumnDescription = "冷却时间")]
	public int CoolingTimeMs { get; set; }

}