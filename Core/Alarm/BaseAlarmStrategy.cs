namespace Core.Alarm;

/// <summary>
/// 策略模式 - 基础策略类，作为报警策略的抽象基类，提供通用的通知逻辑。
/// </summary>
public abstract class BaseAlarmStrategy : IAlarmStrategy
{
    /// <summary>
    /// 表示报警配置对象，包含重复间隔等配置信息。
    /// </summary>
    protected readonly AlarmConfig _config;

    /// <summary>
    /// 表示上一次发送通知的时间，初始值为 DateTime.MinValue。
    /// 用于控制通知的发送频率。
    /// </summary>
    private DateTime _lastSent = DateTime.MinValue;
    
    private readonly object _lock = new object();

    public string Id => string.Empty;

	/// <summary>
	/// 构造函数，初始化基础报警策略并注入配置对象。
	/// </summary>
	/// <param name="config">报警配置对象，提供策略执行所需的配置信息。</param>
	protected BaseAlarmStrategy(AlarmConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// 判断是否应该发送通知，基于配置中的重复间隔时间。
    /// 可被子类重写以实现自定义逻辑。
    /// </summary>
    /// <param name="record">报警记录对象，包含报警的详细信息。</param>
    /// <returns>返回布尔值，表示是否应该发送通知。</returns>
    public virtual bool ShouldNotify(AlarmRecord record)
    {
        lock (_lock)
        {
            var shouldNotify = (DateTime.Now - _lastSent).TotalSeconds > _config.CheckInterval;
            if (shouldNotify) _lastSent = DateTime.Now;
            return shouldNotify;
        }
    }

    /// <summary>
    /// 异步发送报警通知，具体实现由子类提供。
    /// </summary>
    /// <param name="record">报警记录对象，包含需要发送的报警信息。</param>
    /// <param name="type">报警状态，用于指定通知的类型或上下文。</param>
    /// <returns>返回一个 Task，表示异步发送操作的完成。</returns>
    public abstract Task SendAsync(AlarmRecord record);
}