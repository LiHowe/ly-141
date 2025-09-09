namespace Module.Business.BizStrategy;

public class SpotBizStrategyConfig : SimpleStrategyConfig
{
    /// <summary>
    /// 点焊需要监听焊点的变化
    /// </summary>
    public string? SpotIndexNode { get; set; }
}