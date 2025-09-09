using System.IO;

namespace Core;
/// <summary>
/// 通用常量
/// </summary>
public class Constants
{

    /// <summary>
    ///  日志根目录
    /// </summary>
    public static string LogRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    
    public const string ConfigExt = "conf";
    /// <summary>
    /// 配置文件根目录
    /// </summary>
    public static string ConfigRootPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
    
    /// <summary>
    /// 历史查询配置文件根目录
    /// </summary>
    public static string HisConfigFilePath => Path.Combine(ConfigRootPath, $"his.{ConfigExt}");

    /// <summary>
    /// Plc配置文件根目录
    /// </summary>
    public static string PlcConfigFilePath => Path.Combine(ConfigRootPath, $"plc.{ConfigExt}");
    
    /// <summary>
    /// 系统配置文件路径
    /// </summary>
    public static string SystemConfigFilePath => Path.Combine(ConfigRootPath, $"sys.{ConfigExt}");

    /// <summary>
    /// 产品配置文件路径
    /// </summary>
    public static string ProductConfigFilePath => Path.Combine(ConfigRootPath, $"product.{ConfigExt}");
    
    /// <summary>
    /// 数据库配置文件路径
    /// </summary>
    public static string LocalDbConfigFilePath => Path.Combine(ConfigRootPath, $"db.local.{ConfigExt}");
    
    /// <summary>
    /// 数据库配置文件路径 - 螺柱焊 DCE Link
    /// </summary>
    public static string DceDbConfigFilePath => Path.Combine(ConfigRootPath, $"db.dce.{ConfigExt}");
    
    /// <summary>
    /// 数据库配置文件路径 - Bos6000 博士点焊
    /// </summary>
    public static string BosDbConfigFilePath => Path.Combine(ConfigRootPath, $"db.bos.{ConfigExt}");
    
    /// <summary>
    /// 数据库配置文件路径 - WinTB3 那电点焊
    /// </summary>
    public static string WinTB3DbConfigFilePath => Path.Combine(ConfigRootPath, $"db.wintb3.{ConfigExt}");
    
    /// <summary>
    /// Imlight配置文件路径
    /// </summary>
    public static string ImlightConfigFilePath => Path.Combine(ConfigRootPath, $"imlight.{ConfigExt}");
    
    /// <summary>
    /// 模块配置文件路径
    /// </summary>
    public static string ModuleConfigFilePath => Path.Combine(ConfigRootPath, $"module.{ConfigExt}");
    
    public static string ModulesRootPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules");
    
    public static string RepairModuleConfigFilePath => Path.Combine(ModulesRootPath, $"module.repair.{ConfigExt}");
    public static string RivetModuleConfigFilePath => Path.Combine(ModulesRootPath, $"module.rivet.{ConfigExt}");
    public static string SpotModuleConfigFilePath => Path.Combine(ModulesRootPath, $"module.spot.{ConfigExt}");
    public static string ProjectionModuleConfigFilePath => Path.Combine(ModulesRootPath, $"module.projection.{ConfigExt}");
    public static string ArcModuleConfigFilePath => Path.Combine(ModulesRootPath, $"module.arc.{ConfigExt}");
    public static string DceModuleConfigFilePath => Path.Combine(ModulesRootPath, $"module.dce.{ConfigExt}");
    
    
}

public static class SystemEvent
{
    /// <summary>
    /// 配置文件变更
    /// </summary>
    public const string ConfigChanged = "ConfigChanged";

    /// <summary>
    /// 
    /// </summary>
    public const string SystemWarning = "SystemWarning";

    public static string PlcStopped(string plcName) => $"{plcName}.stop";
    public static string PlcResumed(string plcName) => $"{plcName}.resume";
    public static string PlcErrored(string plcName) => $"{plcName}.error";
}