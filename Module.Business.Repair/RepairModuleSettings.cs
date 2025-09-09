using System.IO;
using Core;
using Core.Models;
using Newtonsoft.Json;

namespace Module.Business.Repair;

/// <summary>
/// 返修模块的配置
/// </summary>
public class RepairModuleSettings: ConfigBase
{
    /// <summary>
    /// 触发表达式 - 默认每天下午4点半
    /// </summary>
    public string CronExpression { get; set; } = "0 30 16 * * ?";

    /// <summary>
    /// 是否启用触发
    /// </summary>
    public bool EnableTimer { get; set; } = true;
    
    /// <summary>
    /// 目标节点
    /// </summary>
    public string AlarmNode { get; set; } = "r1.alarm";

    /// <summary>
    /// 目标PLC
    /// </summary>
    public string TargetPlc { get; set; } = "plc1";
    
}