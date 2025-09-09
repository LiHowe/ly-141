using Core.Models;
using SqlSugar;

namespace Core.Alarm;

/// <summary>
/// 报警服务类，提供报警的触发、解决和查询功能。
/// </summary>
public class AlarmRepository
{
    private readonly ISqlSugarClient _db;
    private AlarmCoordinator _notifier;
    

    /// <summary>
    /// 构造函数，初始化报警服务并注入 Sugar 实例以获取数据库客户端。
    /// </summary>
    /// <param name="s">Sugar 实例，用于数据库操作的依赖注入。</param>
    public AlarmRepository(ISqlSugarClient s)
    {
        _db = s; // 通过 Sugar 实例获取 SqlSugarClient 数据库客户端
    }

    #region Coordinator

    public void UseCoordinator(AlarmCoordinator c)
    {
        _notifier = c;
    }
    
    public async void TriggerAlarmAsync(AlarmRecord alarm)
    {
        await _notifier.AlarmAsync(alarm);
        // 启动重复提醒, 当用户一直未解决，则重复提醒
        // TODO: 重复提醒需要设置为配置项 EnableReminder, ReminderInterval
        StartReminderCheck(alarm);
    }
    
    /// <summary>
    ///
    /// TODO: 需要设置timer数量上限，不然会导致内存泄漏
    /// </summary>
    /// <param name="record"></param>
    private void StartReminderCheck(AlarmRecord record)
    {
        // TODO: 切换为配置文件读取
        var timer = new System.Timers.Timer(30000); // 30秒检查
        timer.Elapsed += async (s, e) => 
        {
            if (record.Status == AlarmStatus.Resolved)
            {
                timer.Stop();
                return;
            }
            
            await _notifier.AlarmAsync(record);
        };
        timer.Start();
    }

    #endregion
   
    

    /// <summary>
    /// 异步触发一个新的报警，并将报警记录保存到数据库。
    /// </summary>
    /// <param name="alarmEvent">报警事件对象，包含报警的级别、模块、类别等信息。</param>
    /// <returns>返回保存到数据库中的报警记录实体。</returns>
    public async Task<AlarmRecord> SaveAlarmAsync(AlarmRecord alarmEvent)
    {
        // 将报警记录插入数据库并返回插入后的实体
        return await _db.Insertable(alarmEvent).ExecuteReturnEntityAsync();
    }

    /// <summary>
    /// 异步解决指定 ID 的报警，更新其状态、解决者、备注和解决时间。
    /// </summary>
    /// <param name="alarmId">要解决的报警记录的 ID。</param>
    /// <param name="resolvedBy">解决报警的操作者姓名。</param>
    /// <param name="remarks">解决报警时的备注信息。</param>
    /// <returns>返回一个 Task，表示异步操作的完成。</returns>
    public async Task ResolveAlarmAsync(int alarmId, string? resolvedBy = "", string? remarks = "")
    {
        var record = new AlarmRecord
        {
            Id = alarmId,
            Status = AlarmStatus.Resolved,     // 将状态更新为已解决
            ResolvedBy = resolvedBy,           // 记录解决者的姓名
            Remarks = remarks,                 // 记录备注信息
            ResolvedTime = DateTime.Now        // 设置解决时间为当前时间
        };

        // 更新指定 ID 的报警记录，设置状态为已解决，并记录解决者、备注和当前时间
        var rowsAffected = await _db.Updateable(record)
            .UpdateColumns(r => new
            {
                r.Status,   
                r.ResolvedBy,         
                r.Remarks,             
                r.ResolvedTime
            })
            .ExecuteCommandAsync();                // 执行更新操作
        
        if (rowsAffected == 0) throw new Exception($"Alarm with ID {alarmId} not found or not updated.");
    }

    /// <summary>
    /// 异步查询最近的报警记录，按触发时间降序排序。
    /// </summary>
    /// <param name="topN">返回的记录数量，默认值为 50。</param>
    /// <returns>返回最近的报警记录列表。</returns>
    public async Task<List<AlarmRecord>> QueryRecentAlarmsAsync(int topN = 50)
    {
        // 查询报警记录，按触发时间降序排序，并限制返回指定数量的记录
        return await _db.Queryable<AlarmRecord>()
            .OrderBy(r => r.TriggerTime, OrderByType.Desc) // 按触发时间降序排序
            .Take(topN)                                    // 限制返回记录数量
            .ToListAsync();                                // 转换为列表并返回
    }

    /// <summary>
    /// 异步分页查询报警记录，支持根据级别、模块和开始时间进行筛选。
    /// </summary>
    /// <param name="query">报警查询条件对象，包含级别、模块、开始时间、分页参数等。</param>
    /// <returns>返回分页后的报警记录列表，包含总记录数和分页信息。</returns>
    public async Task<PagedList<AlarmRecord>> QueryAlarmsAsync(AlarmQuery query)
    {
        if (query.PageIndex < 1) query.PageIndex = 1;
        if (query.PageSize < 1) query.PageSize = 50; // 设置合理上限
        RefAsync<int> total = 0; // 定义一个异步引用变量，用于存储总记录数
        var list = await _db.Queryable<AlarmRecord>()
            .WhereIF(query.Levels != null, r => query.Levels.Contains(r.Level)) // 如果指定了级别，则筛选符合条件的记录
            .WhereIF(!string.IsNullOrEmpty(query.Module), r => r.Module == query.Module) // 如果指定了模块，则筛选匹配模块的记录
            .WhereIF(query.StartTime.HasValue, r => r.TriggerTime >= query.StartTime) // 如果指定了开始时间，则筛选触发时间晚于该时间的记录
            .OrderBy(r => r.TriggerTime, OrderByType.Desc) // 按触发时间降序排序
            .ToPageListAsync(query.PageIndex, query.PageSize, total); // 分页查询并返回结果

        // 返回分页列表对象，包含查询结果、总记录数和分页参数
        return new PagedList<AlarmRecord>(list, total, query.PageIndex, query.PageSize);
    }
}