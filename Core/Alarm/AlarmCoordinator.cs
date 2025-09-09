namespace Core.Alarm;

/// <summary>
/// 报警协调器类，负责协调多个报警策略并执行通知操作。
/// </summary>
public class AlarmCoordinator
{

	private static readonly Lazy<AlarmCoordinator> _instance =
	   new Lazy<AlarmCoordinator>(() => new AlarmCoordinator());

	public static AlarmCoordinator Instance => _instance.Value;

	private readonly List<IAlarmStrategy> _strategies = new();
	private readonly object _lock = new();

    public event Action<AlarmRecord> OnNewAlarm;

	public event Action<AlarmRecord> OnResolveAlarm;

    /// <summary>
    /// 构造函数，初始化报警协调器并注入策略集合和数据库客户端。
    /// </summary>
    /// <param name="strategies">报警策略的可枚举集合，定义了不同的通知策略。</param>
    /// <param name="db">SqlSugar 数据库客户端，用于可能的数据库操作。</param>
    public AlarmCoordinator()
    {
    }

	/// <summary>
	/// 添加通知策略
	/// </summary>
	/// <param name="strategy"></param>
	public IAlarmStrategy AddStrategy(IAlarmStrategy strategy)
	{
		lock (_lock)
		{
			//if (!_strategies.Contains(strategy) && !_strategies.Any(x => x.Id == strategy.Id))
			if (!_strategies.Any(x => x.Id == strategy.Id))
			{
				_strategies.Add(strategy);
				return strategy;
			} else
			{
				return _strategies.Find(x => x.Id == strategy.Id)!;
			}
				
		}
	}

	/// <summary>
	/// 异步执行报警通知，协调所有适用的策略并发送通知。
	/// </summary>
	/// <param name="record">报警记录对象，包含需要发送的报警信息。</param>
	/// <param name="type">报警状态，用于指定通知的类型或上下文。</param>
	/// <returns>返回一个 Task，表示所有通知操作的异步完成。</returns>
	public async Task AlarmAsync(AlarmRecord record)
    {
		OnNewAlarm?.Invoke(record);
		List<Task> tasks = new();
		lock (_lock)
		{
			foreach (var strategy in _strategies)
			{
				if (strategy.ShouldNotify(record))
					tasks.Add(strategy.SendAsync(record));
			}
		}
		await Task.WhenAll(tasks);
		//if (_alarmService != null)
		//	await _alarmService.SaveAlarmAsync(record);
	}

	public async Task ResolveAlarmAsync(AlarmRecord record, string? resolvedBy = "System", string? remarks = "批量解决")
	{
		//if (_alarmService != null)
		//	await _alarmService.ResolveAlarmAsync(record.Id, resolvedBy, remarks);
		OnResolveAlarm?.Invoke(record);
	}

	public bool ShouldNotify(AlarmRecord record)
	   => _strategies.Any(s => s.ShouldNotify(record));
}