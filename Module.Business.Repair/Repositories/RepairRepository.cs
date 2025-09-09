using Core.Models;
using SqlSugar;

namespace Module.Business.Repair.Repositories;

public class RepairRepository
{
    private readonly SqlSugarClient _db;
    
    public RepairRepository(SqlSugarClient db)
    {
        _db = db;
    }
    
    /// <summary>
    ///  获取所有返修记录
    /// </summary>
    /// <returns></returns>
    public async Task<List<RepairRecord>> GetAllRepairRecords()
    {
        return await _db.Queryable<RepairRecord>().ToListAsync();
    }
    
    /// <summary>
    ///  获取返修记录分页数据
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public async Task<PagedList<RepairRecord>> GetRepairRecordsPagedAsync(int pageIndex, int pageSize)
    {
        RefAsync<int> total = 0;
        var list = await _db.Queryable<RepairRecord>()
            .OrderBy(x => x.UnloadTime, OrderByType.Desc)
            .ToPageListAsync(pageIndex, pageSize, total);
        return new(list, total, pageIndex, pageSize);
    }

    public async Task<List<RepairRecord>> GetRepairRecordsAsync(
        string? partCode = null,
        DateTime? startTime = null,
        DateTime? endTime = null)
    {
        return await _db.Queryable<RepairRecord>()
            .WhereIF(!string.IsNullOrEmpty(partCode), x => x.PartCode == partCode)
            .WhereIF(startTime.HasValue, x => x.UnloadTime >= startTime)
            .WhereIF(endTime.HasValue, x => x.UnloadTime <= endTime)
            .ToListAsync();
    }

    /// <summary>
    /// 获取今天所有下料未上料的零件
    /// </summary>
    /// <returns></returns>
    public async Task<List<RepairRecord>> GetTodayUnloadRecords()
    {
        return await _db.Queryable<RepairRecord>()
            .Where(x => x.UnloadTime >= DateTime.Today && x.LoadTime == null)
            .ToListAsync();
    }

    /// <summary>
    /// 获取今天所有下料并正常上料的零件
    /// </summary>
    /// <returns></returns>
    public async Task<List<RepairRecord>> GetTodayReloadRecords()
    {
       return await _db.Queryable<RepairRecord>()
            .Where(x => x.LoadTime >= DateTime.Today && x.LoadTime != null)
            .ToListAsync();
    }
    
    /// <summary>
    /// 获取今日统计数据
    /// </summary>
    /// <returns></returns>
    public async Task<(int,int)> GetTodayStatistics()
    {
        var unloadCount = await GetTodayUnloadRecords();
        var reloadCount = await GetTodayReloadRecords();
        return (unloadCount.Count, reloadCount.Count);
    }
    
    public void AddRepairRecord(RepairRecord record)
    {
        _db.Insertable(record).ExecuteCommand();
    }
    
    public void UpdateRepairRecord(RepairRecord record)
    {
        _db.Updateable(record).ExecuteCommand();
    }
    
    /// <summary>
    /// 根据零件码添加下料记录
    /// </summary>
    /// <param name="partCode"></param>
    public void AddUnloadRecord(string partCode)
    {
        var record = new RepairRecord
        {
            PartCode = partCode,
            UnloadTime = DateTime.Now
        };
        _db.Insertable(record).ExecuteCommand();
    }
    
    /// <summary>
    ///  根据零件码更新上料时间
    /// </summary>
    /// <param name="partCode"></param>
    /// <param name="loadTime"></param>
    public void UpdateRecordLoadTime(string partCode, DateTime loadTime)
    {
        _db.Updateable<RepairRecord>().SetColumns(x => new RepairRecord { LoadTime = loadTime })
            .Where(x => x.PartCode == partCode)
            .ExecuteCommand();
    }
    
    /// <summary>
    /// 根据零件码判断是否存在返修记录
    /// </summary>
    /// <param name="serialNo"></param>
    /// <returns></returns>
    public bool ExistRepairRecord(string serialNo)
    {
        return _db.Queryable<RepairRecord>().Any(x => x.SerialNo == serialNo);
    }
}