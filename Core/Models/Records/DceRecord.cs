using SqlSugar;

namespace Core.Models.Records;

/// <summary>
/// DCE LINK 螺柱焊模型, 对应DCE数据库的Welds表
/// </summary>
[SugarTable(TableDescription = "DCE LINK 螺柱焊")]  // 数据库表名
public class DceStudRecord : RecordBase
{
	/// <summary>
	/// Welds 表的主键
	/// </summary>
	[SugarColumn(ColumnDescription = "ID")]
	public int WeldID { get; set; }  // int

	/// <summary>
	/// 顺序号
	/// </summary>
	[SugarColumn(ColumnDescription = "顺序号")]
	public int nRecordID { get; set; }  // int

	/// <summary>
	/// 日期时间
	/// </summary>
	[SugarColumn(ColumnDescription = "日期时间")]
	public DateTime Timew { get; set; }  // datetime

	/// <summary>
	/// 版本号
	/// </summary>
	[SugarColumn(ColumnDescription = "版本号")]
	public short nVersion { get; set; }  // smallint

	/// <summary>
	/// 组（该设备属于哪个一个组）
	/// </summary>
	[SugarColumn(Length = 255, IsNullable = true, ColumnDescription = "设备组")]
	public string? sWelderGroups { get; set; }  // nvarchar(255)

	/// <summary>
	/// 设备名称
	/// </summary>
	[SugarColumn(Length = 50, ColumnDescription = "设备名称")]
	public string Welder { get; set; } = string.Empty;  // nvarchar(50)

	/// <summary>
	/// 端口号（1号枪/2号枪）
	/// </summary>
	[SugarColumn(ColumnDescription = "端口号")]
	public short Outlet { get; set; }  // smallint

	/// <summary>
	/// ID号（程序号）
	/// </summary>
	[SugarColumn(Length = 16, ColumnDescription = "程序号")]
	public string StudID { get; set; } = string.Empty;  // nvarchar(16)

	/// <summary>
	/// SMPS故障
	/// </summary>
	[SugarColumn(ColumnDescription = "SMPS故障")]
	public bool SMPSFault { get; set; }  // bit

	/// <summary>
	/// 无提升错误（无意义）
	/// </summary>
	[SugarColumn(ColumnDescription = "无提升错误")]
	public bool NoLiftFault { get; set; }  // bit

	/// <summary>
	/// 短路错误
	/// </summary>
	[SugarColumn(ColumnDescription = "短路错误")]
	public bool ShortCircuitFault { get; set; }  // bit

	/// <summary>
	/// 下落超时
	/// </summary>
	[SugarColumn(ColumnDescription = "下落超时")]
	public bool DropdownTimeout { get; set; }  // bit

	/// <summary>
	/// 开路错误
	/// </summary>
	[SugarColumn(ColumnDescription = "开路错误")]
	public bool OpenCircuitFault { get; set; }  // bit

	/// <summary>
	/// 焊接电流检测值1（smallint）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电流检测值1")]
	public short ActualWeldCurrent1 { get; set; }  // smallint

	/// <summary>
	/// 焊接时间检测值（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接时间检测值")]
	public double ActualWeldTime { get; set; }  // float -> double

	/// <summary>
	/// 下落时间检测值（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "下落时间检测值")]
	public double DropTime { get; set; }  // float -> double

	/// <summary>
	/// 引弧电压检测值（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "引弧电压检测值")]
	public double PilotArcVoltage { get; set; }  // float -> double

	/// <summary>
	/// 焊接电压检测值1（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电压检测值")]
	public double MainArcVoltage1 { get; set; }  // float -> double

	/// <summary>
	/// 优化模式下焊接电流（smallint）
	/// </summary>
	[SugarColumn(ColumnDescription = "优化模式焊接电流")]
	public short OptimizedTargetWeldCurrent { get; set; }  // smallint

	/// <summary>
	/// 优化模式下焊接时间（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "优化模式焊接时间")]
	public double OptimizedTargetWeldTime { get; set; }  // float -> double

	/// <summary>
	/// 引弧电流检测值（smallint）
	/// </summary>
	[SugarColumn(ColumnDescription = "引弧电流检测值")]
	public short ActualPilotCurrent { get; set; }  // smallint

	/// <summary>
	/// 能量（smallint）
	/// </summary>
	[SugarColumn(ColumnDescription = "能量")]
	public short energy { get; set; }  // smallint

	/// <summary>
	/// 焊接电流设定值1（smallint）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电流设定值")]
	public short TargetWeldCurrent1 { get; set; }  // smallint

	/// <summary>
	/// 焊接电流正公差（smallint）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电流正公差")]
	public short WeldCurrentPlusTolerance { get; set; }  // smallint

	/// <summary>
	/// 焊接电流负公差（smallint）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电流负公差")]
	public short WeldCurrentMinusTolerance { get; set; }  // smallint

	/// <summary>
	/// 焊接时间设定值（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接时间设定值")]
	public double TargetWeldTime { get; set; }  // float -> double

	/// <summary>
	/// 焊接时间正公差（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接时间正公差")]
	public double WeldTimePlusTolerance { get; set; }  // float -> double

	/// <summary>
	/// 焊接时间负公差（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接时间负公差")]
	public double WeldTimeMinusTolerance { get; set; }  // float -> double

	/// <summary>
	/// 熔深设定值（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "熔深设定值")]
	public double PenetrationTarget { get; set; }  // float -> double

	/// <summary>
	/// 熔深正公差（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "熔深正公差")]
	public double PenetrationPlusTolerance { get; set; }  // float -> double

	/// <summary>
	/// 熔深负公差（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "熔深负公差")]
	public double PenetrationMinusTolerance { get; set; }  // float -> double

	/// <summary>
	/// 引弧电压目标值（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "引弧电压目标值")]
	public double TargetPilotArcVoltage { get; set; }  // float -> double

	/// <summary>
	/// 引弧电压正公差（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "引弧电压正公差")]
	public double PilotVoltagePlusTolerance { get; set; }  // float -> double

	/// <summary>
	/// 引弧电压负公差（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "引弧电压负公差")]
	public double PilotVoltageMinusTolerance { get; set; }  // float -> double

	/// <summary>
	/// 焊接电压目标值（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电压目标值")]
	public double TargetMainArcVoltage { get; set; }  // float -> double

	/// <summary>
	/// 焊接电压正公差（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电压正公差")]
	public double MainVpt { get; set; }  // float -> double

	/// <summary>
	/// 焊接电压负公差（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电压负公差")]
	public double MainArcVoltageMinusTolerance { get; set; }  // float -> double

	/// <summary>
	/// 提升高度设定值（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "提升高度设定值")]
	public double TargetLiftHeight { get; set; }  // float -> double

	/// <summary>
	/// 提升高度正公差（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "提升高度正公差")]
	public double LiftHeightPlusTolerance { get; set; }  // float -> double

	/// <summary>
	/// 提升高度负公差（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "提升高度负公差")]
	public double LiftHeightMinusTolerance { get; set; }  // float -> double

	/// <summary>
	/// 允许超差次数（smallint）
	/// </summary>
	[SugarColumn(ColumnDescription = "允许超差次数")]
	public short AllowedWOPs { get; set; }  // smallint

	/// <summary>
	/// 提升高度检测值（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "提升高度检测值")]
	public double LiftHeight { get; set; }  // float -> double

	/// <summary>
	/// 下落时间目标值（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "下落时间目标值")]
	public double TargetDropTime { get; set; }  // float -> double

	/// <summary>
	/// 焊接电流超差（bit）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电流超差")]
	public bool WeldCurrentOT { get; set; }  // bit

	/// <summary>
	/// 焊接时间超差（bit）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接时间超差")]
	public bool WeldTimeOT { get; set; }  // bit

	/// <summary>
	/// 焊接电压超差（bit）
	/// </summary>
	[SugarColumn(ColumnDescription = "焊接电压超差")]
	public bool MainVoltOT { get; set; }  // bit

	/// <summary>
	/// 引弧电压超差（bit）
	/// </summary>
	[SugarColumn(ColumnDescription = "引弧电压超差")]
	public bool PilotVoltOT { get; set; }  // bit

	/// <summary>
	/// 提升高度超差（bit）
	/// </summary>
	[SugarColumn(ColumnDescription = "提升高度超差")]
	public bool LiftHeightOT { get; set; }  // bit

	/// <summary>
	/// 下落时间超差（bit）
	/// </summary>
	[SugarColumn(ColumnDescription = "下落时间超差")]
	public bool DroptimeOT { get; set; }  // bit

	/// <summary>
	/// 熔深超差（bit）
	/// </summary>
	[SugarColumn(ColumnDescription = "熔深超差")]
	public bool PenetrationOT { get; set; }  // bit

	/// <summary>
	/// 能量超差（bit）
	/// </summary>
	[SugarColumn(ColumnDescription = "能量超差")]
	public bool EnergyOT { get; set; }  // bit

	/// <summary>
	/// 熔深实际值（float）
	/// </summary>
	[SugarColumn(ColumnDescription = "熔深实际值")]
	public double Penetration { get; set; }  // float -> double

	/// <summary>
	/// 伸出长度有效（bit）
	/// </summary>
	[SugarColumn(ColumnDescription = "伸出长度有效")]
	public bool bStickOutValid { get; set; }  // bit

	/// <summary>
	/// 下落力模式（bit）
	/// </summary>
	[SugarColumn(ColumnDescription = "下落力模式")]
	public bool ForceMode { get; set; }  // bit

	/// <summary>
	/// 下落位置模式（bit）
	/// </summary>
	[SugarColumn(ColumnDescription = "下落位置模式")]
	public bool PositionMode { get; set; }  // bit

	/// <summary>
	/// 能量目标值（smallint）
	/// </summary>
	[SugarColumn(ColumnDescription = "能量目标值")]
	public short energytarget { get; set; }  // smallint

	/// <summary>
	/// 能量正公差（smallint）
	/// </summary>
	[SugarColumn(ColumnDescription = "能量正公差")]
	public short energyplustolerance { get; set; }  // smallint

	/// <summary>
	/// 能量负公差（smallint）
	/// </summary>
	[SugarColumn(ColumnDescription = "能量负公差")]
	public short energyminustolerance { get; set; }  // smallint

	/// <summary>
	/// 车身号
	/// </summary>
	[SugarColumn(Length = 16, IsNullable = true, ColumnDescription = "车身号")]
	public string? CarID { get; set; }  // nvarchar(16)
}