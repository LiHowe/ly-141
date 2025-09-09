using Core.Models;
using SqlSugar;

namespace Module.Business.Rivet;

/// <summary>
/// 拉铆记录仓库
/// </summary>
public class RivetRepository(SqlSugarClient db)
{
    /// <summary>
    /// 添加拉铆记录
    /// </summary>
    public async Task AddRivetRecordAsync(RivetRecord record)
    {
        await db.Insertable(record).ExecuteCommandAsync();
    }

    /// <summary>
    /// 添加拉铆记录
    /// </summary>
    /// <param name="serialNo"></param>
    /// <param name="force"></param>
    /// <param name="displacement"></param>
    /// <param name="rivetIndex"></param>
    /// <param name="rivetName"></param>
    public async Task AddRivetRecordAsync(string serialNo, double force, double displacement, int rivetIndex = 0, string rivetName = "")
    {
        var record = new RivetRecord
        {
            SerialNo = serialNo,
            Force = force,
            Displacement = displacement,
            RivetIndex = rivetIndex,
            RivetName = rivetName
        };
        await db.Insertable(record).ExecuteCommandAsync();
    }
    
    public async Task<List<RivetRecord>> GetAllRivetRecords()
    {
        return await db.Queryable<RivetRecord>().ToListAsync();
    }

    /// <summary>
    /// 根据条件获取拉铆记录
    /// </summary>
    /// <param name="serialNo"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public async Task<List<RivetRecord>> GetRivetRecordAsync(string? serialNo = null, DateTime? startTime = null, DateTime? endTime = null)
    {
        return await db.Queryable<RivetRecord>()
            .WhereIF(!string.IsNullOrEmpty(serialNo), x => x.SerialNo == serialNo)
            .WhereIF(startTime.HasValue, x => x.CreateTime >= startTime)
            .WhereIF(endTime.HasValue, x => x.CreateTime <= endTime)
            .ToListAsync();
    }
    
    /// <summary>
    ///  获取拉铆记录分页数据
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <param name="serialNo"></param>
    /// <returns></returns>
    public async Task<PagedList<RivetRecord>> GetRivetRecordsPagedAsync(int pageIndex, int pageSize, string? serialNo = null)
    {
        RefAsync<int> total = 0;
        var list = await db.Queryable<RivetRecord>()
            .WhereIF(!string.IsNullOrEmpty(serialNo), x => x.SerialNo == serialNo)
            .OrderBy(x => x.CreateTime, OrderByType.Desc)
            .ToPageListAsync(pageIndex, pageSize, total);
        return new(list, total, pageIndex, pageSize);
    }
    
    public void UpdateRivetRecord(RivetRecord record)
    {
        db.Updateable(record).ExecuteCommand();
    }
}