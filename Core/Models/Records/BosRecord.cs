using AutoMapper;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Core.Models.Records;

[SugarTable(TableDescription = "BOS点焊记录表")]
public class BosRecord : BizRecordBase
{
	/// <summary>
	/// 记录ID，主键，用于唯一标识每条点焊记录，可能是自增字段。
	/// </summary>
	[SugarColumn(ColumnDescription = "记录ID")]
	public int ProtRecord_ID { get; set; }

	/// <summary>
	/// 记录时间，标识数据记录的生成时间，通常包括日期和时间戳。
	/// </summary>
	[SugarColumn(ColumnDescription = "记录时间")]
	public DateTime DateTime { get; set; }

	/// <summary>
	/// 计时器名称，可能是指定焊接周期或计时器的标识符。
	/// </summary>
	[SugarColumn(ColumnDescription = "计时器名称", IsNullable = true)]
	public string? TimerName { get; set; } = string.Empty;

	/// <summary>
	/// 程序编号，标识使用的焊接程序或参数集。
	/// </summary>
	[SugarColumn(ColumnDescription = "程序编号", IsNullable = true)]
	public int? ProgNo { get; set; } // 可为空，因为可能是可选字段

	/// <summary>
	/// 焊接点名称，标识具体的焊接位置或点位名称。
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接点名称")]
	public string SpotName { get; set; } = string.Empty;

	/// <summary>
	/// 第一阶段需求电流，设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第一阶段需求电流", IsNullable = true)]
	public decimal? IDemand1 { get; set; }

	/// <summary>
	/// 第一阶段实际电流，测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第一阶段实际电流", IsNullable = true)]
	public decimal? IActual1 { get; set; }

	/// <summary>
	/// 第二阶段需求电流，设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第二阶段需求电流", IsNullable = true)]
	public decimal? IDemand2 { get; set; }

	/// <summary>
	/// 第二阶段实际电流，测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第二阶段实际电流", IsNullable = true)]
	public decimal? IActual2 { get; set; }

	/// <summary>
	/// 第三阶段需求电流，设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第三阶段需求电流", IsNullable = true)]
	public decimal? IDemand3 { get; set; }

	/// <summary>
	/// 第三阶段实际电流，测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第三阶段实际电流", IsNullable = true)]
	public decimal? IActual3 { get; set; }

	/// <summary>
	/// 第一阶段相位值，测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第一阶段相位值", IsNullable = true)]
	public decimal? Pha1 { get; set; }

	/// <summary>
	/// 第二阶段相位值，测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第二阶段相位值", IsNullable = true)]
	public decimal? Pha2 { get; set; }

	/// <summary>
	/// 第三阶段相位值，测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第三阶段相位值", IsNullable = true)]
	public decimal? Pha3 { get; set; }

	/// <summary>
	/// 实际电压值（伏特），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际电压值", IsNullable = true)]
	public decimal? VoltageActualValue { get; set; }

	/// <summary>
	/// 实际电流值，测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际电流值", IsNullable = true)]
	public decimal? CurrentActualValue { get; set; }

	/// <summary>
	/// 实际焊接时间（毫秒或周期），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际焊接时间", IsNullable = true)]
	public decimal? WeldTimeActualValue { get; set; }

	/// <summary>
	/// 实际能量值（焦耳），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际能量值", IsNullable = true)]
	public decimal? EnergyActualValue { get; set; }

	/// <summary>
	/// 实际功率值（瓦），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际功率值", IsNullable = true)]
	public decimal? PowerActualValue { get; set; }

	/// <summary>
	/// 实际电阻值（欧姆），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际电阻值", IsNullable = true)]
	public decimal? ResistanceActualValue { get; set; }


	private MapperConfiguration config = new(cfg =>
	{
		cfg.CreateMap<BosOriginRecord, BosRecord>();
	}, new LoggerFactory());

	public BosRecord ConvertFromBosOriginRecord(BosOriginRecord record)
	{
		return config.CreateMapper().Map<BosRecord>(record);
	}
}
