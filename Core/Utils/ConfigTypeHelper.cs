using Core.Interfaces;

namespace Core.Utils
{
    /// <summary>
    /// 配置类型识别工具类
    /// 根据配置文件路径识别配置类型
    /// </summary>
    public static class ConfigTypeHelper
    {
        /// <summary>
        /// 根据配置文件路径获取配置类型
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        /// <returns>配置类型</returns>
        public static ConfigType GetConfigType(string configPath)
        {
            if (string.IsNullOrWhiteSpace(configPath))
                return ConfigType.Unknown;

            // 标准化路径分隔符
            var normalizedPath = configPath.Replace('\\', '/').ToLowerInvariant();

            // 根据Constants中定义的路径进行匹配
            if (normalizedPath.Contains("sys.") || normalizedPath.Contains("system"))
                return ConfigType.System;

            if (normalizedPath.Contains("db.local") || normalizedPath.Contains("database"))
                return ConfigType.Database;

            if (normalizedPath.Contains("plc."))
                return ConfigType.Plc;

            if (normalizedPath.Contains("his.") || normalizedPath.Contains("history"))
                return ConfigType.History;

            if (normalizedPath.Contains("product."))
                return ConfigType.Product;

            if (normalizedPath.Contains("imlight."))
                return ConfigType.Imlight;

            if (normalizedPath.Contains("db.dce"))
                return ConfigType.DceDatabase;

            if (normalizedPath.Contains("db.bos"))
                return ConfigType.BosDatabase;

            if (normalizedPath.Contains("db.wintb3"))
                return ConfigType.WinTB3Database;
            
            if (normalizedPath.Contains("module.repair"))
                return ConfigType.RepairModule;
            if (normalizedPath.Contains("module.rivet"))
                return ConfigType.RivetModule;
            if (normalizedPath.Contains("module.spot"))
                return ConfigType.SpotModule;
            if (normalizedPath.Contains("module.projection"))
                return ConfigType.ProjectionModule;
            if (normalizedPath.Contains("module.arc"))
                return ConfigType.ArcModule;
            if (normalizedPath.Contains("module.dce"))
                return ConfigType.DceModule;
            return ConfigType.Unknown;
        }

        /// <summary>
        /// 根据配置文件路径获取配置类型名称
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        /// <returns>配置类型名称</returns>
        public static string GetConfigTypeName(string configPath)
        {
            var configType = GetConfigType(configPath);
            return GetConfigTypeName(configType);
        }

        /// <summary>
        /// 获取配置类型的显示名称
        /// </summary>
        /// <param name="configType">配置类型</param>
        /// <returns>配置类型显示名称</returns>
        public static string GetConfigTypeName(ConfigType configType)
        {
            return configType switch
            {
                ConfigType.System => "系统配置",
                ConfigType.Database => "数据库配置",
                ConfigType.Plc => "PLC配置",
                ConfigType.History => "历史查询配置",
                ConfigType.Product => "产品配置",
                ConfigType.Imlight => "Imlight配置",
                ConfigType.DceDatabase => "DCE数据库配置",
                ConfigType.BosDatabase => "BOS数据库配置",
                ConfigType.WinTB3Database => "WinTB3数据库配置",
                ConfigType.RepairModule => "返修模块配置",
                ConfigType.RivetModule => "拉铆模块配置",
                ConfigType.SpotModule => "点焊模块配置",
                ConfigType.ProjectionModule => "凸焊模块配置",
                ConfigType.ArcModule => "弧焊模块配置",
                ConfigType.DceModule => "DCE模块配置",
                _ => "未知配置"
            };
        }

        /// <summary>
        /// 检查配置路径是否匹配指定的配置类型
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        /// <param name="expectedType">期望的配置类型</param>
        /// <returns>是否匹配</returns>
        public static bool IsConfigType(string configPath, ConfigType expectedType)
        {
            return GetConfigType(configPath) == expectedType;
        }

        /// <summary>
        /// 根据Constants中的路径常量精确匹配配置类型
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        /// <returns>配置类型</returns>
        public static ConfigType GetConfigTypeByConstants(string configPath)
        {
            if (string.IsNullOrWhiteSpace(configPath))
                return ConfigType.Unknown;

            // 标准化路径
            var normalizedPath = configPath.Replace('\\', '/');

            if (Constants.SystemConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.System;

            if (Constants.LocalDbConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.Database;

            if (Constants.PlcConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.Plc;

            if (Constants.HisConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.History;

            if (Constants.ProductConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.Product;

            if (Constants.ImlightConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.Imlight;

            if (Constants.DceDbConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.DceDatabase;

            if (Constants.BosDbConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.BosDatabase;

            if (Constants.WinTB3DbConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.WinTB3Database;
            
            if (Constants.RepairModuleConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.RepairModule;
            
            if (Constants.RivetModuleConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.RivetModule;
            
            if (Constants.SpotModuleConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.SpotModule;
            
            if (Constants.ProjectionModuleConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.ProjectionModule;
            
            if (Constants.ArcModuleConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.ArcModule;
            
            if (Constants.DceModuleConfigFilePath.EndsWith(normalizedPath, StringComparison.OrdinalIgnoreCase))
                return ConfigType.DceModule;
            
            return ConfigType.Unknown;
        }
    }
}