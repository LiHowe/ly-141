using Connection.S7;
using UI.Controls;
using UI.ViewModels;

namespace Module.Business.BizStrategy;

public interface IBizStrategyConfig
{
    
    #region PLC相关

    /// <summary>
    /// PLC配置
    /// </summary>
    public S7PlcConfig? PlcConfig { get; set; }

    /// <summary>
    /// PLC配置对应的PLC
    /// </summary>
    public S7Plc? Plc  => PlcConfig?.GetPlc();
    
    /// <summary>
    ///  业务目标PLC
    /// </summary>
    public string TargetPlcKey { get; set; }


    /// <summary>
    /// 开始节点
    /// </summary>
    public string? StartNodeKey { get; set; }

    /// <summary>
    /// 结束节点
    /// </summary>
    public string? EndNodeKey { get; set; }

    /// <summary>
    /// 零件码扫码结果节点
    /// </summary>
    public string? CodeNodeKey { get; set; }

    /// <summary>
    /// 数据点
    /// </summary>
    public List<string> DataNodeKeys { get; }
    
    /// <summary>
    /// 流程模式
    /// </summary>
    public StrategyStartType Mode { get; set; }

    #endregion
    
    /// <summary>
    /// 配置验证方法
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public bool ValidateConfig(out string errorMessage);
}