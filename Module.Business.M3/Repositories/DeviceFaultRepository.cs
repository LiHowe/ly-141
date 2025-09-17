using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using Module.Business.SG141.Models;
using SqlSugar;

namespace Module.Business.SG141.Repositories
{
	public class DeviceFaultRepository
	{

		ISqlSugarClient _db;
		
		public DeviceFaultRepository(ISqlSugarClient db)
		{
			_db = db;
		}
		
		public List<DeviceFaultRecord> GetAll()
		{
			return _db.Queryable<DeviceFaultRecord>().ToList();
		}

		/// <summary>
		/// 分页查询
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public async Task<PagedList<DeviceFaultRecord>> GetPagedListAsync(DeviceFaultQuery query)
		{
			RefAsync<int> totalCount = 0;
			
			var res = await _db.Queryable<DeviceFaultRecord>()
				.WhereIF(!string.IsNullOrWhiteSpace(query.DeviceName),
					record => record.DeviceName.Contains(query.DeviceName))
				.WhereIF(!string.IsNullOrWhiteSpace(query.FaultType), record => record.FaultType == query.FaultType)
				.WhereIF(!string.IsNullOrWhiteSpace(query.StartTime), record => record.FaultTime >= DateTime.Parse(query.StartTime))
				.WhereIF(!string.IsNullOrWhiteSpace(query.EndTime), record => record.FaultTime <= DateTime.Parse(query.EndTime))
				.ToPageListAsync(query.PageIndex, query.PageSize, totalCount);
			
			return new PagedList<DeviceFaultRecord>(res, totalCount, query.PageIndex, query.PageSize);
		}

		/// <summary>
		/// 获取指定日期的设备故障统计数据, 默认获取当天数据
		/// </summary>
		/// <param name="dateTime"></param>
		public async Task<List<DeviceFaultStatistic>> GetStatisticData(DateTime? dateTime = null)
		{
			dateTime ??= DateTime.Today;
			
			return await _db.Queryable<DeviceFaultRecord>()
				.Where(record => record.FaultTime.Date >= dateTime.Value.Date && record.FaultTime.Date <= dateTime.Value.Date)
				.GroupBy(record => new { record.DeviceName })
				.Select(it => new DeviceFaultStatistic
				{
					DeviceName = it.DeviceName,     // 分组键
					Count = SqlFunc.AggregateCount(it.DeviceName)  // 统计数量
				})
				.MergeTable()//需要加MergeTable才能排序统计过的列
				.OrderBy(it=>it.Count)
				.ToListAsync();
		}
		
		public DeviceFaultRecord GetById(int id)
		{
			return _db.Queryable<DeviceFaultRecord>().InSingle(id);
		}
		
		/// <summary>
		///  添加设备故障记录
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int Insert(DeviceFaultRecord entity)
		{
			return _db.Insertable(entity).ExecuteReturnIdentity();
		}

		/// <summary>
		/// 添加设备故障记录
		/// </summary>
		/// <param name="deviceName"></param>
		/// <param name="faultType"></param>
		/// <returns></returns>
		public int Insert(string deviceName, string faultType)
		{
			var entity = new DeviceFaultRecord()
			{
				DeviceName = deviceName,
				FaultTime = DateTime.Now,
				FaultType = faultType
			};
			return Insert(entity);
		}
		
	}
}
