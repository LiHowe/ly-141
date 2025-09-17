using Core.Models;
using SqlSugar;

namespace Module.Business.SG141.Models
{
	/// <summary>
	/// 设备故障记录
	/// </summary>
	[SugarTable(TableDescription = "设备故障记录")]
	public class DeviceFaultRecord: RecordBase
	{
		[SugarColumn(ColumnDescription = "设备名称")]
		public string DeviceName { get; set; } = string.Empty;

		[SugarColumn(ColumnDescription = "设备码", IsNullable = true)]
		public string DeviceCode { get; set; } = string.Empty;

		[SugarColumn(ColumnDescription = "故障时间")]
		public DateTime FaultTime { get; set; }

		public string FaultTimeStr => FaultTime.ToString("yyyy-MM-dd HH:mm:ss");

		[SugarColumn(ColumnDescription = "故障类型", IsNullable = true)]
		public string FaultType { get; set; } = string.Empty;

		[SugarColumn(ColumnDescription = "故障描述", IsNullable = true)]
		public string FaultDescription { get; set; } = string.Empty;
	}
}
