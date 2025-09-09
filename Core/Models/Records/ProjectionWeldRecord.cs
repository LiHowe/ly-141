using SqlSugar;

namespace Core.Models.Records;

[SugarTable(TableDescription = "凸焊记录表")]
public class ProjectionWeldRecord : BizRecordBase
{
	

	#region 阶段1 - 预压， 通常无电流通过
	/// <summary>
	/// 阶段1放电时间
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段1放电时间", IsNullable = true)]
	public int WeldTime1 { get; set; }

	/// <summary>
	/// 阶段1电流峰值
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段1电流有效值", IsNullable = true)]
	public double CurrentRMS1 { get; set; }
	/// <summary>
	/// 阶段1电流平均值
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段1电流峰值", IsNullable = true)]
	public double CurrentPeak1 { get; set; }

	/// <summary>
	/// 焊接电压（伏特）
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段1焊接电压", IsNullable = true)]
	public double Voltage1 { get; set; }

	/// <summary>
	/// 焊接功率(阶段2) 瓦
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段1焊接功率", IsNullable = true)]
	public double Power1 => Voltage1 * CurrentRMS1;

	/// <summary>
	/// 焊接能量 千焦耳
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段1焊接能量", IsNullable = true)]
	public double Energy1 => Power1 * WeldTime1;

	#endregion

	#region 阶段2 - 主焊接阶段
	/// <summary>
	/// 阶段2放电时间
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段2放电时间", IsNullable = true)]
	public int WeldTime2 { get; set; }
	/// <summary>
	/// 阶段2焊接电流（熔核）
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段2电流有效值", IsNullable = true)]
	public double CurrentRMS2 { get; set; }

	/// <summary>
	/// 阶段2焊接电流（熔核）
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段2电流峰值", IsNullable = true)]
	public double CurrentPeak2 { get; set; }
	/// <summary>
	/// 焊接电压（伏特）
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段2焊接电压", IsNullable = true)]
	public double Voltage2 { get; set; }

	/// <summary>
	/// 焊接功率(阶段2) 瓦
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段2焊接功率", IsNullable = true)]
	public double Power2 => Voltage2 * CurrentRMS2;

	/// <summary>
	/// 焊接能量 千焦耳
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段2焊接能量", IsNullable = true)]
	public double Energy2 => Power2 * WeldTime2;

	#endregion


	#region 阶段3 - 冷却阶段
	/// <summary>
	/// 阶段3放电时间
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段3放电时间", IsNullable = true)]
	public int WeldTime3 { get; set; }
	/// <summary>
	/// 阶段3焊接电流
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段3电流有效值", IsNullable = true)]
	public double CurrentRMS3 { get; set; }

	/// <summary>
	/// 阶段3焊接电流
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段3电流峰值", IsNullable = true)]
	public double CurrentPeak3 { get; set; }

	/// <summary>
	/// 焊接电压（伏特）
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段3焊接电压", IsNullable = true)]
	public double Voltage3 { get; set; }

	/// <summary>
	/// 焊接功率(阶段3) 瓦
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段3焊接功率", IsNullable = true)]
	public double Power3 => Voltage3 * CurrentRMS3;

	/// <summary>
	/// 焊接能量 千焦耳
	/// </summary>
	[SugarColumn(ColumnDescription = "阶段3焊接能量", IsNullable = true)]
	public double Energy3 => Power3 * WeldTime3;

	#endregion

	/// <summary>
	/// 点位
	/// </summary>
	[SugarColumn(ColumnDescription = "点位", Length = 10)]
	public string PointIndex { get; set; } = string.Empty;

	/// <summary>
	/// 电阻值（欧姆）
	/// </summary>
	[SugarColumn(ColumnDescription = "电阻", IsNullable = true)]
	public double Resistance { get; set; }

	/// <summary>
	/// 电极力（牛顿）
	/// </summary>
	[SugarColumn(ColumnDescription = "电极压力", IsNullable = true)]
	public double ElectrodeForce { get; set; }

	/// <summary>
	/// 材料类型
	/// </summary>
	[SugarColumn(ColumnDescription = "材料类型", IsNullable = true)]
	public string? MaterialType { get; set; }

	/// <summary>
	/// 材料厚度（毫米）
	/// </summary>
	[SugarColumn(ColumnDescription = "材料厚度", IsNullable = true)]
	public double Thickness { get; set; }

	/// <summary>
	/// 凸点高度（毫米）
	/// </summary>
	[SugarColumn(ColumnDescription = "凸点高度", IsNullable = true)]
	public double BumpHeight { get; set; }

	/// <summary>
	/// 凸点直径（毫米）
	/// </summary>
	[SugarColumn(ColumnDescription = "凸点直径", IsNullable = true)]
	public double BumpDiameter { get; set; }
}