using System.Configuration;
using Config;
using Core.Models;

namespace Module.Business.Rivet;

/// <summary>
/// 拉铆模块配置
/// </summary>
public class RivetModuleSettings: ConfigBase
{
    public List<RivetModuleSetting> Configs { get; set; } = new();
}

public class RivetModuleSetting : ConfigBase
{
    /// <summary>
    /// 目标节点
    /// </summary>
    public string AlarmNode { get; set; } = "r1.alarm";

    /// <summary>
    /// 目标PLC
    /// </summary>
    public string TargetPlc { get; set; } = "plc1";
}