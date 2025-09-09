using SqlSugar;

namespace Core.Models.Records;

/// <summary>
/// 对应 ExtWeldMeasureProt_RDS_V 视图字段
/// </summary>
[SugarTable("ExtWeldMeasureProt_RDS_V")]
public class BosOriginRecord
{
	/// <summary>
	/// 记录ID，主键，用于唯一标识每条点焊记录，可能是自增字段。
	/// </summary>
	[SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "记录ID")]
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
	/// 电极磨损值，记录电极的使用磨损程度（单位可能为毫米或百分比）。
	/// </summary>
	[SugarColumn(ColumnDescription = "电极磨损值", IsNullable = true)]
	public decimal? Wear { get; set; }

	/// <summary>
	/// 电极磨损百分比，电极磨损程度占总使用寿命的百分比。
	/// </summary>
	[SugarColumn(ColumnDescription = "电极磨损百分比", IsNullable = true)]
	public decimal? WearPerCent { get; set; }

	/// <summary>
	/// 监控状态的数值表示，可能是状态码（如 0 表示正常，1 表示异常）。
	/// </summary>
	[SugarColumn(ColumnDescription = "监控状态", IsNullable = true)]
	public int? MonitorState { get; set; }

	/// <summary>
	/// 监控状态的文本描述，如“Normal”、“Fault”等，对应 MonitorState。
	/// </summary>
	[SugarColumn(ColumnDescription = "监控状态文本描述")]
	public string MonitorState_txt { get; set; } = string.Empty;

	/// <summary>
	/// 调节状态的数值表示，指示电流或电压调节的当前状态。
	/// </summary>
	[SugarColumn(ColumnDescription = "调节状态", IsNullable = true)]
	public int? RegulationState { get; set; }

	/// <summary>
	/// 调节状态的文本描述，如“Regulating”、“Stable”等。
	/// </summary>
	[SugarColumn(ColumnDescription = "调节状态文本描述")]
	public string RegulationState_txt { get; set; } = string.Empty;

	/// <summary>
	/// 测量状态的数值表示，指示测量过程的状态。
	/// </summary>
	[SugarColumn(ColumnDescription = "测量状态", IsNullable = true)]
	public int? MeasureState { get; set; }

	/// <summary>
	/// 测量状态的文本描述，如“Measuring”、“Complete”等。
	/// </summary>
	[SugarColumn(ColumnDescription = "测量状态文本描述")]
	public string MeasureState_txt { get; set; } = string.Empty;

	/// <summary>
	/// 电源状态的数值表示，指示电源是否正常工作。
	/// </summary>
	[SugarColumn(ColumnDescription = "电源状态", IsNullable = true)]
	public int? PowerState { get; set; }

	/// <summary>
	/// 电源状态的文本描述，如“On”、“Off”或“Error”。
	/// </summary>
	[SugarColumn(ColumnDescription = "电源状态文本描述")]
	public string PowerState_txt { get; set; } = string.Empty;

	/// <summary>
	/// 序列状态的数值表示，指示焊接序列的当前阶段。
	/// </summary>
	[SugarColumn(ColumnDescription = "序列状态", IsNullable = true)]
	public int? SequenceState { get; set; }

	/// <summary>
	/// 序列状态的文本描述，如“Start”、“InProgress”、“End”。
	/// </summary>
	[SugarColumn(ColumnDescription = "序列状态文本描述")]
	public string SequenceState_txt { get; set; } = string.Empty;

	/// <summary>
	/// 附加序列状态，可能表示序列的额外状态或子状态。
	/// </summary>
	[SugarColumn(ColumnDescription = "附加序列状态", IsNullable = true)]
	public int? SequenceStateAdd { get; set; }

	/// <summary>
	/// 附加序列状态的文本描述。
	/// </summary>
	[SugarColumn(ColumnDescription = "附加序列状态文本描述")]
	public string SequenceStateAdd_txt { get; set; } = string.Empty;

	/// <summary>
	/// 序列重复次数，记录焊接序列的重复执行次数。
	/// </summary>
	[SugarColumn(ColumnDescription = "序列重复次数", IsNullable = true)]
	public int? SequenceRepeat { get; set; }

	/// <summary>
	/// 序列重复次数的文本描述或状态。
	/// </summary>
	[SugarColumn(ColumnDescription = "序列重复次数文本描述")]
	public string SequenceRepeat_txt { get; set; } = string.Empty;

	/// <summary>
	/// 监控模式，可能是手动、自动或其他监控设置的模式。
	/// </summary>
	[SugarColumn(ColumnDescription = "监控模式", IsNullable = true)]
	public int? MonitorMode { get; set; }

	/// <summary>
	/// 监控模式的文本描述，如“Auto”、“Manual”。
	/// </summary>
	[SugarColumn(ColumnDescription = "监控模式文本描述")]
	public string MonitorMode_txt { get; set; } = string.Empty;

	/// <summary>
	/// 标准需求电流（安培），设定值，表示期望的焊接电流。
	/// </summary>
	[SugarColumn(ColumnDescription = "标准需求电流（安培）", IsNullable = true)]
	public decimal? IDemandStd { get; set; }

	/// <summary>
	/// 电流状态，可能表示当前电流的状态值或平均值。
	/// </summary>
	[SugarColumn(ColumnDescription = "电流状态", IsNullable = true)]
	public decimal? Ilsts { get; set; }

	/// <summary>
	/// 标准调节值，可能为调节参数的设定值或标识。
	/// </summary>
	[SugarColumn(ColumnDescription = "标准调节值", IsNullable = true)]
	public decimal? RegulationStd { get; set; }

	/// <summary>
	/// 标准调节值的文本描述。
	/// </summary>
	[SugarColumn(ColumnDescription = "标准调节值文本描述")]
	public string RegulationStd_txt { get; set; } = string.Empty;

	/// <summary>
	/// 第一阶段需求电流（安培），设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第一阶段需求电流（安培）", IsNullable = true)]
	public decimal? IDemand1 { get; set; }

	/// <summary>
	/// 第一阶段实际电流（安培），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第一阶段实际电流（安培）", IsNullable = true)]
	public decimal? IActual1 { get; set; }

	/// <summary>
	/// 第一阶段调节状态或值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第一阶段调节状态", IsNullable = true)]
	public int? Regulation1 { get; set; }

	/// <summary>
	/// 第一阶段调节状态的文本描述。
	/// </summary>
	[SugarColumn(ColumnDescription = "第一阶段调节状态文本描述")]
	public string Regulation1_txt { get; set; } = string.Empty;

	/// <summary>
	/// 第二阶段需求电流（安培），设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第二阶段需求电流（安培）", IsNullable = true)]
	public decimal? IDemand2 { get; set; }

	/// <summary>
	/// 第二阶段实际电流（安培），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第二阶段实际电流（安培）", IsNullable = true)]
	public decimal? IActual2 { get; set; }

	/// <summary>
	/// 第二阶段调节状态或值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第二阶段调节状态", IsNullable = true)]
	public int? Regulation2 { get; set; }

	/// <summary>
	/// 第二阶段调节状态的文本描述。
	/// </summary>
	[SugarColumn(ColumnDescription = "第二阶段调节状态文本描述")]
	public string Regulation2_txt { get; set; } = string.Empty;

	/// <summary>
	/// 第三阶段需求电流（安培），设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第三阶段需求电流（安培）", IsNullable = true)]
	public decimal? IDemand3 { get; set; }

	/// <summary>
	/// 第三阶段实际电流（安培），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第三阶段实际电流（安培）", IsNullable = true)]
	public decimal? IActual3 { get; set; }

	/// <summary>
	/// 第三阶段调节状态或值。
	/// </summary>
	[SugarColumn(ColumnDescription = "第三阶段调节状态", IsNullable = true)]
	public int? Regulation3 { get; set; }

	/// <summary>
	/// 第三阶段调节状态的文本描述。
	/// </summary>
	[SugarColumn(ColumnDescription = "第三阶段调节状态文本描述")]
	public string Regulation3_txt { get; set; } = string.Empty;

	/// <summary>
	/// 标准相位，可能是参考相位角或功率因数的设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "标准相位", IsNullable = true)]
	public decimal? PhaStd { get; set; }

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
	/// 标准需求时间，可能为焊接时间的设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "标准需求时间", IsNullable = true)]
	public decimal? T_iDemandStd { get; set; }

	/// <summary>
	/// 实际标准时间，测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际标准时间", IsNullable = true)]
	public decimal? TActualStd { get; set; }

	/// <summary>
	/// 零件标识字符串，标识被焊接工件的唯一标识符。
	/// </summary>
	[SugarColumn(ColumnDescription = "零件标识字符串")]
	public string PartIdentString { get; set; } = string.Empty;

	/// <summary>
	/// 电极修整计数器，记录电极修整的次数。
	/// </summary>
	[SugarColumn(ColumnDescription = "电极修整计数器", IsNullable = true)]
	public int? TipDressCounter { get; set; }

	/// <summary>
	/// 电极编号，标识使用的电极。
	/// </summary>
	[SugarColumn(ColumnDescription = "电极编号", IsNullable = true)]
	public int? ElectrodeNo { get; set; }

	/// <summary>
	/// 实际电压值（伏特），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际电压值（伏特）", IsNullable = true)]
	public decimal? VoltageActualValue { get; set; }

	/// <summary>
	/// 参考电压值（伏特），设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "参考电压值（伏特）", IsNullable = true)]
	public decimal? VoltageRefValue { get; set; }

	/// <summary>
	/// 实际电流值（安培），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际电流值（安培）", IsNullable = true)]
	public decimal? CurrentActualValue { get; set; }

	/// <summary>
	/// 参考电流值（安培），设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "参考电流值（安培）", IsNullable = true)]
	public decimal? CurrentReferenceValue { get; set; }

	/// <summary>
	/// 实际焊接时间（毫秒或周期），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际焊接时间", IsNullable = true)]
	public decimal? WeldTimeActualValue { get; set; }

	/// <summary>
	/// 参考焊接时间（毫秒或周期），设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "参考焊接时间", IsNullable = true)]
	public decimal? WeldTimeRefValue { get; set; }

	/// <summary>
	/// 实际能量值（焦耳），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际能量值（焦耳）", IsNullable = true)]
	public decimal? EnergyActualValue { get; set; }

	/// <summary>
	/// 参考能量值（焦耳），设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "参考能量值（焦耳）", IsNullable = true)]
	public decimal? EnergyRefValue { get; set; }

	/// <summary>
	/// 实际功率值（瓦），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际功率值（瓦）", IsNullable = true)]
	public decimal? PowerActualValue { get; set; }

	/// <summary>
	/// 参考功率值（瓦），设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "参考功率值（瓦）", IsNullable = true)]
	public decimal? PowerRefValue { get; set; }

	/// <summary>
	/// 实际电阻值（欧姆），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际电阻值（欧姆）", IsNullable = true)]
	public decimal? ResistanceActualValue { get; set; }

	/// <summary>
	/// 参考电阻值（欧姆），设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "参考电阻值（欧姆）", IsNullable = true)]
	public decimal? ResistanceRefValue { get; set; }

	/// <summary>
	/// 实际脉冲宽度（毫秒），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际脉冲宽度（毫秒）", IsNullable = true)]
	public decimal? PulseWidthActualValue { get; set; }

	/// <summary>
	/// 参考脉冲宽度（毫秒），设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "参考脉冲宽度（毫秒）", IsNullable = true)]
	public decimal? PulseWidthRefValue { get; set; }

	// ---------------------------- 下面字段很少设置采集 ---------------------------------
	// 如果需要采集， 则需BOS设置中开启 UIR 监控

	/// <summary>
	/// 实际稳定因子值，测量值，反映焊接稳定性。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际稳定因子值", IsNullable = true)]
	public decimal? StabilisationFactorActValue { get; set; }

	/// <summary>
	/// 参考稳定因子值，设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "参考稳定因子值", IsNullable = true)]
	public decimal? StabilisationFactorRefValue { get; set; }

	/// <summary>
	/// 稳定因子阈值，用于判断焊接是否稳定。
	/// </summary>
	[SugarColumn(ColumnDescription = "稳定因子阈值", IsNullable = true)]
	public decimal? ThresholdStabilisationFactor { get; set; }

	/// <summary>
	/// 焊接效果稳定因子，反映焊接质量的稳定程度。
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接效果稳定因子", IsNullable = true)]
	public decimal? WldEffectStabilisationFactor { get; set; }

	/// <summary>
	/// 实际 UIP 值（可能为电压-电流-功率的组合），测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "实际 UIP 值", IsNullable = true)]
	public decimal? UipActualValue { get; set; }

	/// <summary>
	/// 参考 UIP 值，设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "参考 UIP 值", IsNullable = true)]
	public decimal? UipRefValue { get; set; }

	/// <summary>
	/// 喷溅时间（毫秒），记录焊接过程中发生的喷溅持续时间。
	/// </summary>
	[SugarColumn(ColumnDescription = "喷溅时间（毫秒）", IsNullable = true)]
	public int? UirExpulsionTime { get; set; }

	/// <summary>
	/// 测量活动状态，指示是否启用测量。
	/// </summary>
	[SugarColumn(ColumnDescription = "测量活动状态", IsNullable = true)]
	public int? UirMeasuringActive { get; set; }

	/// <summary>
	/// 测量活动状态的文本描述，如“Active”、“Inactive”。
	/// </summary>
	[SugarColumn(ColumnDescription = "测量活动状态文本描述")]
	public string UirMeasuringActive_txt { get; set; } = string.Empty;

	/// <summary>
	/// 调节活动状态，指示是否启用调节。
	/// </summary>
	[SugarColumn(ColumnDescription = "调节活动状态", IsNullable = true)]
	public int? UirRegulationActive { get; set; }

	/// <summary>
	/// 调节活动状态的文本描述。
	/// </summary>
	[SugarColumn(ColumnDescription = "调节活动状态文本描述")]
	public string UirRegulationActive_txt { get; set; } = string.Empty;

	/// <summary>
	/// 监控活动状态，指示是否启用监控。
	/// </summary>
	[SugarColumn(ColumnDescription = "监控活动状态", IsNullable = true)]
	public int? UirMonitoringActive { get; set; }

	/// <summary>
	/// 监控活动状态的文本描述。
	/// </summary>
	[SugarColumn(ColumnDescription = "监控活动状态文本描述")]
	public string UirMonitoringActive_txt { get; set; } = string.Empty;

	/// <summary>
	/// 焊接时间延长活动状态，指示是否延长焊接时间。
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接时间延长活动状态", IsNullable = true)]
	public int? UirWeldTimeProlongationActive { get; set; }

	/// <summary>
	/// 焊接时间延长活动状态的文本描述。
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接时间延长活动状态文本描述")]
	public string UirWeldTimeProlongActive_txt { get; set; } = string.Empty;

	/// <summary>
	/// 质量停止参考计数值，设定值。
	/// </summary>
	[SugarColumn(ColumnDescription = "质量停止参考计数值", IsNullable = true)]
	public int? UirQStoppRefCntValue { get; set; }

	/// <summary>
	/// 质量停止实际计数值，测量值。
	/// </summary>
	[SugarColumn(ColumnDescription = "质量停止实际计数值", IsNullable = true)]
	public int? UirQStoppActCntValue { get; set; }

	/// <summary>
	/// UIP 上限容差，允许的UIP最大值偏差。
	/// </summary>
	[SugarColumn(ColumnDescription = "UIP 上限容差", IsNullable = true)]
	public decimal? UirUipUpperTol { get; set; }

	/// <summary>
	/// UIP 下限容差，允许的UIP最小值偏差。
	/// </summary>
	[SugarColumn(ColumnDescription = "UIP 下限容差", IsNullable = true)]
	public decimal? UirUipLowerTol { get; set; }

	/// <summary>
	/// UIP 条件容差，可能用于特定条件下的容差设置。
	/// </summary>
	[SugarColumn(ColumnDescription = "UIP 条件容差", IsNullable = true)]
	public decimal? UirUipCondTol { get; set; }

	/// <summary>
	/// 功率稳定因子下限容差，允许的最小稳定因子值。
	/// </summary>
	[SugarColumn(ColumnDescription = "功率稳定因子下限容差", IsNullable = true)]
	public decimal? UirPsfLowerTol { get; set; }

	/// <summary>
	/// 功率稳定因子条件容差，特定条件下的容差设置。
	/// </summary>
	[SugarColumn(ColumnDescription = "功率稳定因子条件容差", IsNullable = true)]
	public decimal? UirPsfCondTol { get; set; }
}
