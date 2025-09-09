namespace Core.Models;


/// <summary>
/// 作为查询业务记录的基类
/// </summary>
public class RecordQueryBase
{
	/// <summary>
	/// 根据ID精确查询产品数据
	/// </summary>
	public int? Id { get; set; }
	
	/// <summary>
	/// 零件码查询，通常用于唯一标识某个零件
	/// </summary>
	public string? SerialNo { get; set; }

	/// <summary>
	/// 工序编号，用于指定生产或加工的具体工序
	/// </summary>
	public string? ProcessNo { get; set; }

	/// <summary>
	/// 批次号，用于区分不同生产批次
	/// </summary>
	public string? BatchNo { get; set; }
	
	/// <summary>
	///  工作站，用于区分不同的生产或加工地点
	/// </summary>
	public string? Station { get; set; }

	/// <summary>
	/// 查询的起始时间，通常用于筛选时间范围内的数据
	/// </summary>
	public DateTime? StartTime { get; set; }

	/// <summary>
	/// 查询的结束时间，通常用于筛选时间范围内的数据
	/// </summary>
	public DateTime? EndTime { get; set; }

	/// <summary>
	/// 备注信息，可用于模糊查询或附加说明
	/// </summary>
	public string? Remark { get; set; }

	/// <summary>
	/// 质量状态，如合格、不合格等
	/// </summary>
	public string? Quality { get; set; }

	/// <summary>
	/// 当前页码，用于分页查询
	/// </summary>
	public int CurrentPage { get; set; }

	/// <summary>
	/// 每页显示的数据条数，用于分页查询
	/// </summary>
	public int PageSize { get; set; }
}
