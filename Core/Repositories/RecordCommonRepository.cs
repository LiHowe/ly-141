using Core.Models;
using Data.SqlSugar;

namespace Core.Repositories;

/// <summary>
/// 记录通用仓库
/// </summary>
public class RecordCommonRepository<T> where T : RecordBase, new()
{

    private Timer _timer;

    private readonly Sugar _sugar;

    public RecordCommonRepository(Sugar s)
    {
        _sugar = s;
        Record = new T();
        _timer = new((state) =>
        {

        }, null, Timeout.Infinite, Timeout.Infinite);	
    }

    public T Record { get; set; }

    /// <summary>
    /// 保存记录，并生产一个新记录
    /// </summary>
    /// <returns></returns>
    public T SaveAndCreateRecord()
    {
        _sugar.GetDb().Insert<T>(Record);
        Record = new();
        return Record;
    }

    /// <summary>
    /// 根据序列号更新记录的产品编号SerialNo字段
    /// </summary>
    /// <param name="sn"></param>
    /// <returns>更新是否成功</returns>
    public bool UpdatePn(string oPn, string npn)
    {
        var records = _sugar.GetDb().Queryable<T>()
            .Where(x => x.SerialNo == oPn)
            .ToList();

        if (records.Count == 0) return false;
        foreach (var record in records)
        {
            record.SerialNo = npn;
        }

        var res = _sugar.GetDb().Updateable(records)
            .UpdateColumns(x => new { x.SerialNo })
            .ExecuteCommand();
        return res != 0;
    }

	

}
