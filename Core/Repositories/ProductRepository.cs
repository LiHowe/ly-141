using Core.Models;
using Core.Models.Records;
using Data.SqlSugar;
using SqlSugar;

namespace Core.Repositories;

public class ProductRepository
{
    private readonly Sugar _sugar;

    public ProductRepository(Sugar local)
    {
        _sugar = local;
    }

    #region 公共方法

    public void Save(ProductRecord record)
    {
        _sugar.GetDb().InsertOrUpdate(record);
    }

    public void SaveWhenNotExist(ProductRecord record)
    {
        // 如果存在，直接返回
        if (_sugar.GetDb().Queryable<ProductRecord>().Any(x => x.SerialNo == record.SerialNo)) return;
        _sugar.GetDb().Insert(record);
    }

    /// <summary>
    /// 根据序列号或产品编号获取已有记录
    /// </summary>
    /// <param name="snOrPn"></param>
    public ProductRecord? GetExistingRecord(string snOrPn)
    {
        return _sugar.GetDb().Queryable<ProductRecord>()
            .Where(x => x.SerialNo == snOrPn || x.ProcessNo == snOrPn)
            .First();
    }

    public List<ProductRecord> GetAll()
    {
        return _sugar.GetDb().GetAll<ProductRecord>();
    }

    public List<ProductRecord> GetPaged(int page, int pageSize, ref int total, ref int totalPage)
    {
        return _sugar.GetDb().GetPaged<ProductRecord>(page, pageSize, ref total, ref totalPage);
    }
    
    /// <summary>
    /// 条件分页查询
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<PagedList<ProductRecord>> GetPagedAsync(RecordQueryBase query)
    {
        RefAsync<int> total = 0;
        var list =  await _sugar.GetDb().Queryable<ProductRecord>()
            .WhereIF(!string.IsNullOrEmpty(query.SerialNo), x => x.SerialNo.Contains(query.SerialNo))
            .WhereIF(query.StartTime.HasValue, x => x.CreateTime >= query.StartTime)
            .WhereIF(query.EndTime.HasValue, x => x.CreateTime <= query.EndTime)
            .WhereIF(!string.IsNullOrEmpty(query.Quality), x => x.Quality == query.Quality)
            .OrderBy(x => x.CreateTime, OrderByType.Desc)
            .ToPageListAsync(query.CurrentPage, query.PageSize, total);
        return new(list, total, query.CurrentPage, query.PageSize);
    }

    public int GetCount()
    {
        return _sugar.GetDb().GetCount<ProductRecord>();
    }

    /// <summary>
    /// 获取NG与OK的分组产品数量
    /// </summary>
    /// <returns></returns>
    public (int, int) GetQualityCount()
    {
        var okCount = _sugar.GetDb().Queryable<ProductRecord>().Where(x => x.Quality == "OK").Count();
        var ngCount = _sugar.GetDb().Queryable<ProductRecord>().Where(x => x.Quality == "NG").Count();
        return (okCount, ngCount);
    }


    /// <summary>
    /// 获取每日的产量
    /// </summary>
    /// <param name="date">指定日期，默认为当日</param>
    /// <returns></returns>
    public int GetDailyCount(DateTime? date = null)
    {
        date ??= DateTime.Today;
        var end = date.Value.AddDays(1);
        return _sugar.GetDb().Queryable<ProductRecord>()
            .Where(x => x.CreateTime >= date && x.CreateTime <= end)
            .Count();
    }

    /// <summary>
    /// 获取一周的产量数据
    /// </summary>
    /// <param name="date">指定日期</param>
    /// <returns></returns>
    public Dictionary<DateTime, int> GetWeekCount(DateTime? date = null)
    {
        DateTime today = date ?? DateTime.Today;
        Dictionary<DateTime, int> res = new();
        for (int i = 0; i < 7; i++)
        {
            // 计算近7天的日期
            DateTime d = today.AddDays(-i);
            int count = GetDailyCount(d);
            res.Add(d, count);
        }

        return res;
    }

    /// <summary>
    /// 计算平均节拍, 默认取最近20件进行节拍计算)
    /// </summary>
    /// <returns></returns>
    public double GetAverageRate()
    {
        List<DateTime> dates = _sugar.GetDb().Queryable<ProductRecord>()
            .Where(x => x.CreateTime >= DateTime.Today)
            .Select(x => x.CreateTime)
            .ToList();
        if (dates == null || dates.Count <= 1)
            return 0;

        // 先对日期排序
        var sortedDates = dates.OrderBy(d => d).ToList();

        // 计算所有相邻日期之间的时间差
        List<TimeSpan> timeSpans = new List<TimeSpan>();
        for (int i = 1; i < sortedDates.Count; i++)
        {
            timeSpans.Add(sortedDates[i] - sortedDates[i - 1]);
        }

        // 计算时间间隔的Ticks值
        var ticksList = timeSpans.Select(ts => ts.Ticks).ToList();

        // 计算Q1、Q3和IQR
        double Q1 = GetPercentile(ticksList, 25);
        double Q3 = GetPercentile(ticksList, 75);
        double IQR = Q3 - Q1;

        // 计算异常值的上下界限
        double lowerBound = Q1 - 1.5 * IQR;
        double upperBound = Q3 + 1.5 * IQR;

        // 过滤掉异常值
        var filteredTicks = ticksList.Where(t => t >= lowerBound && t <= upperBound).ToList();

        // 计算剔除异常值后的平均时间间隔
        if (filteredTicks.Count > 0)
        {
            return filteredTicks.Average() / 10_000_000.0;
        }
        else
        {
            return 0f; // 如果没有有效数据，则返回零
        }
    }

// 计算给定百分位数的函数
    double GetPercentile(List<long> sortedData, double percentile)
    {
        if (sortedData == null || sortedData.Count == 0)
            throw new ArgumentException("The data list cannot be null or empty.");

        // 排序数据（确保是升序排列）
        var sorted = sortedData.OrderBy(x => x).ToList();

        // 计算百分位数的位置（百分位数值为 0 到 100）
        double rank = percentile / 100.0 * (sorted.Count + 1);

        if (rank < 1) rank = 1;
        if (rank > sorted.Count) rank = sorted.Count;

        // 使用插值法获取百分位数的值
        int lowerIndex = (int)(rank) - 1;
        int upperIndex = lowerIndex + 1;

        if (upperIndex >= sorted.Count)
        {
            return sorted[lowerIndex];
        }

        // 线性插值
        double lowerValue = sorted[lowerIndex];
        double upperValue = sorted[upperIndex];
        return lowerValue + (rank - (lowerIndex + 1)) * (upperValue - lowerValue);
    }

    public double GetQualifiedRate(DateTime? date = null)
    {
        date ??= DateTime.Today;
        var okCount = _sugar.GetDb().Queryable<ProductRecord>().Where(x => x.Quality == "OK" && x.CreateTime >= date)
            .Count();
        var ngCount = _sugar.GetDb().Queryable<ProductRecord>().Where(x => x.Quality == "NG" && x.CreateTime >= date)
            .Count();
        if (okCount == 0) return 0;
        if (ngCount == 0) return 100;
        return okCount / (okCount + ngCount) * 100.0;
    }

    #endregion
}