namespace Core.Alarm;

public class AlarmConfig
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// 相同报警重复提醒间隔
    /// </summary>
    public int CheckInterval { get; set; } = 30 * 1000;

    /// <summary>
    /// 是否开启实时推送
    /// </summary>
    public bool EnableRealtime { get; set; } = true;
}