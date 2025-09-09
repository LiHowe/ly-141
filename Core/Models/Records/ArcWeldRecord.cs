using SqlSugar;

namespace Core.Models.Records;

[SugarTable(TableDescription = "弧焊记录表")]
public class ArcWeldRecord : BizRecordBase
{
	/// <summary>
	/// 焊接电流（安培）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电流")]
	public double WeldingCurrent { get; set; }

	/// <summary>
	/// 焊接电压（伏特）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电压")]
	public double WeldingVoltage { get; set; }

	/// <summary>
	/// 焊接速度（毫米/秒）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接速度")]
	public double TravelSpeed { get; set; }

	/// <summary>
	/// 电极类型（如E6010）
	/// </summary>
	[SugarColumn(ColumnDescription = "电极类型", IsNullable = true)]
	public string? ElectrodeType { get; set; }

	/// <summary>
	/// 电极直径（毫米）
	/// </summary>
	[SugarColumn(ColumnDescription = "电极直径")]
	public double ElectrodeDiameter { get; set; }

	/// <summary>
	/// 保护气体类型（如氩气）
	/// </summary>
	[SugarColumn(ColumnDescription = "保护气体类型", IsNullable = true)]
	public string? ShieldingGasType { get; set; }

	/// <summary>
	/// 保护气体流量（升/分钟）
	/// </summary>
	[SugarColumn(ColumnDescription = "保护气体流量")]
	public double ShieldingGasFlowRate { get; set; }

	/// <summary>
	/// 行进角度（度）
	/// </summary>
	[SugarColumn(ColumnDescription = "行进角度")]
	public double TravelAngle { get; set; }

	/// <summary>
	/// 工作角度（度）
	/// </summary>
	[SugarColumn(ColumnDescription = "工作角度")]
	public double WorkAngle { get; set; }

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
}