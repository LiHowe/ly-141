using System.ComponentModel;

namespace Module.Business.BizStrategy;

public interface IBizStrategy
{
    #region 事件

    /// <summary>
    /// 收到开始信号后
    /// </summary>
    public event EventHandler<BizStrategyEventArgs>? Started;
    /// <summary>
    /// 收到结束信号后
    /// </summary>
    public event EventHandler<BizStrategyEventArgs>? Ended;
    
    /// <summary>
    /// 采集到扫码信息后
    /// </summary>
    public event EventHandler<SimpleStringEventArgs>? CodeReceived; 
    
    /// <summary>
    /// 采集到业务数据后
    /// </summary>
    public event EventHandler<BizStrategyDataReceivedEventArgs>? DataReceived;
    
    /// <summary>
    /// 发生错误时调用
    /// </summary>
    public event EventHandler<BizStrategyEventArgs>? ErrorOccurred;

    #endregion  
    
    public string Name { get; set; }
    public string Description { get; set; }
    
    public Task Apply(IBizStrategyConfig c);

}

/// <summary>
/// 流程开始类型
/// </summary>
public enum StrategyStartType
{
    [Description("使用开始与结束节点")] StartEnd,
    [Description("使用Code节点")] CodeAsStart,
    [Description("使用开始节点作为开始结束")] StartOnly,
    [Description("仅使用结束节点")] EndOnly, // 适用于读取群控数据，比如流程结束后才会有数据的情况
}