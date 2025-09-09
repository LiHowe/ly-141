using System.ComponentModel;
using System.Text;
using Connection.S7;
using Core;
using Core.Models.Settings;
using Core.Utils;
using UI.Controls;
using UI.ViewModels;

namespace Module.Business.BizStrategy;

/// <summary>
/// 简单策略配置, 比较常见的逻辑场景，如果需要特殊场景推荐自行编码
/// </summary>
public class SimpleStrategyConfig : IBizStrategyConfig
{
    /// <summary>
    /// 流程模式
    /// </summary>
    public StrategyStartType Mode { get; set; } = StrategyStartType.StartEnd;

    #region PLC相关

    /// <summary>
    /// PLC配置
    /// </summary>
    public S7PlcConfig? PlcConfig { get; set; }

    /// <summary>
    /// PLC配置对应的PLC
    /// </summary>
    public S7Plc? Plc => PlcConfig?.GetPlc();

    /// <summary>
    ///  业务目标PLC
    /// </summary>
    public string TargetPlcKey { get; set; } = string.Empty;

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
    public List<string> DataNodeKeys { get; private set; } = new();

    #endregion

    public SimpleStrategyConfig()
    {
        if (PlcConfig == null)
        {
            // 使用TargetPlcKey来获取配置
            PlcConfig = ConfigManager.Instance.LoadConfig<PlcSettings>(Constants.PlcConfigFilePath)
                ?.Get(TargetPlcKey);
        }
    }

    /// <summary>
    /// 配置验证方法
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    public virtual bool ValidateConfig(out string errorMessage)
    {
        var errors = new StringBuilder();

        if (PlcConfig is null) errors.AppendLine("PLCConfig cannot be null.");

        switch (Mode)
        {
            case StrategyStartType.StartEnd:
                if (string.IsNullOrWhiteSpace(StartNodeKey))
                    errors.AppendLine("StartNode cannot be null.");
                if (string.IsNullOrWhiteSpace(EndNodeKey))
                    errors.AppendLine("EndNode cannot be null.");
                break;

            case StrategyStartType.CodeAsStart:
                if (string.IsNullOrWhiteSpace(CodeNodeKey))
                    errors.AppendLine("CodeNode cannot be null.");
                if (string.IsNullOrWhiteSpace(EndNodeKey))
                    errors.AppendLine("EndNode cannot be null.");
                break;

            case StrategyStartType.StartOnly:
                if (string.IsNullOrWhiteSpace(StartNodeKey))
                    errors.AppendLine("StartNode cannot be null.");
                break;

            default:
                break;
        }

        errorMessage = errors.ToString();
        return errorMessage.Length == 0;
    }
}