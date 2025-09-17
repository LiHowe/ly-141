using Module.Business.SG141.Models;
using SqlSugar;

namespace Module.Business.SG141.Repositories;

/// <summary>
///    生产统计仓储
/// </summary>
public class ProductStatisticRepository
{
    private readonly ISqlSugarClient _db;
    
    public ProductStatisticRepository(ISqlSugarClient db)
    {
        _db = db;
    }
    
    public List<ProductStatisticRecord> GetAll()
    {
        return _db.Queryable<ProductStatisticRecord>().ToList();
    }

    /// <summary>
    /// 查询某一天的统计数据
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public async Task<List<ProductStatisticRecord>> GetAll(DateTime? date = null)
    {
        date ??= DateTime.Today;
        return await _db.Queryable<ProductStatisticRecord>()
            .Where(record => record.StatisticTime >= date.Value && record.StatisticTime <= date.Value.AddDays(1))
            .OrderBy(r => r.StatisticTime)
            .ToListAsync();
    }

    /// <summary>
    ///    添加统计数据
    /// </summary>
    /// <param name="date"></param>
    /// <param name="ok"></param>
    /// <param name="ng"></param>
    public async Task AddStatisticRecord(DateTime date, int ok, int ng)
    {
        await _db.Insertable(new ProductStatisticRecord
        {
            StatisticTime = date,
            OkCount = ok,
            NgCount = ng
        }).ExecuteCommandAsync();
    }
}