using Core.Models;

namespace Module.Business;

/// <summary>
/// 业务模块服务注册
/// </summary>
public class WeldBusinessModule: ModuleBase
{
    public override string ModuleId { get; } = "Module.Business.WeldBusinessModule";
    public override string ModuleName { get; } = "焊接业务逻辑";
    
    /// <summary>
    /// 模块版本
    /// </summary>
    public override string Version => "1.0.0";

    /// <summary>
    /// 模块描述
    /// </summary>
    public override string Description => "焊接机器人状态监控和数据展示模块，支持实时参数显示和图表分析";

    
}