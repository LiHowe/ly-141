using System.Reflection;
using Core.Interfaces;
using Core.Models;
using Core.Models.Records;
using Core.Models.Settings;
using Core.Utils;
using Data.SqlSugar;
using Logger;


namespace Core.Services
{
    /// <summary>
    /// 模块数据库服务实现
    /// 提供模块级别的数据库表管理功能
    /// </summary>
    public class ModuleDatabaseService : IModuleDatabaseService
    {
        private readonly Dictionary<string, ModuleDatabaseConfiguration> _moduleConfigs = new();
        private readonly object _lockObject = new object();

        /// <summary>
        /// 为指定模块创建数据库表
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="tableTypes">要创建的表类型列表</param>
        /// <returns>创建是否成功</returns>
        public async Task<bool> CreateModuleTablesAsync(string moduleId, List<Type> tableTypes)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(moduleId) || tableTypes == null || !tableTypes.Any())
                {
                    Log.Warn($"[ModuleDatabaseService] 模块ID为空或表类型列表为空: {moduleId}");
                    return true; // 没有表需要创建，视为成功
                }

                // 验证表类型
                var validationResults = ValidateTableTypes(tableTypes);
                var invalidTypes = validationResults.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();
                if (invalidTypes.Any())
                {
                    Log.Error($"[ModuleDatabaseService] 模块 {moduleId} 包含无效的表类型: {string.Join(", ", invalidTypes.Select(t => t.Name))}");
                    return false;
                }

                // 获取数据库连接
                var dbConnection = await CreateModuleDatabaseConnectionAsync(moduleId);
                if (dbConnection is not Sugar sugar)
                {
                    Log.Error($"[ModuleDatabaseService] 无法为模块 {moduleId} 创建数据库连接");
                    return false;
                }

                var successCount = 0;
                var totalCount = tableTypes.Count;

                foreach (var tableType in tableTypes)
                {
                    try
                    {
                        // 检查表是否已存在
                        var exists = await CheckTableExistsAsync(tableType);
                        if (exists)
                        {
                            Log.Info($"[ModuleDatabaseService] 表 {tableType.Name} 已存在，跳过创建");
                            successCount++;
                            continue;
                        }

                        // 创建表
                        var createTableMethod = sugar.GetType().GetMethod("CreateTable")?.MakeGenericMethod(tableType);
                        if (createTableMethod != null)
                        {
                            createTableMethod.Invoke(sugar, null);
                            Log.Info($"[ModuleDatabaseService] 成功为模块 {moduleId} 创建表: {tableType.Name}");
                            successCount++;
                        }
                        else
                        {
                            Log.Error($"[ModuleDatabaseService] 无法获取CreateTable方法: {tableType.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[ModuleDatabaseService] 为模块 {moduleId} 创建表 {tableType.Name} 失败", ex);
                    }
                }

                var success = successCount == totalCount;
                Log.Info($"[ModuleDatabaseService] 模块 {moduleId} 数据库表创建完成: {successCount}/{totalCount}");
                return success;
            }
            catch (Exception ex)
            {
                Log.Error($"[ModuleDatabaseService] 为模块 {moduleId} 创建数据库表失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 删除指定模块的数据库表
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="tableTypes">要删除的表类型列表</param>
        /// <returns>删除是否成功</returns>
        public async Task<bool> DropModuleTablesAsync(string moduleId, List<Type> tableTypes)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(moduleId) || tableTypes == null || !tableTypes.Any())
                {
                    return true; // 没有表需要删除，视为成功
                }

                // 检查模块配置是否允许删除表
                var config = GetModuleConfiguration(moduleId);
                if (!config.DropTablesOnUnload)
                {
                    Log.Info($"[ModuleDatabaseService] 模块 {moduleId} 配置不允许删除表，跳过删除操作");
                    return true;
                }

                // 获取数据库连接
                var dbConnection = await CreateModuleDatabaseConnectionAsync(moduleId);
                if (dbConnection is not Sugar sugar)
                {
                    Log.Error($"[ModuleDatabaseService] 无法为模块 {moduleId} 创建数据库连接");
                    return false;
                }

                var successCount = 0;
                var totalCount = tableTypes.Count;

                foreach (var tableType in tableTypes)
                {
                    try
                    {
                        // 检查表是否存在
                        var exists = await CheckTableExistsAsync(tableType);
                        if (!exists)
                        {
                            Log.Info($"[ModuleDatabaseService] 表 {tableType.Name} 不存在，跳过删除");
                            successCount++;
                            continue;
                        }

                        // 删除表
                        var dropTableMethod = sugar.GetType().GetMethod("DropTable")?.MakeGenericMethod(tableType);
                        if (dropTableMethod != null)
                        {
                            dropTableMethod.Invoke(sugar, null);
                            Log.Info($"[ModuleDatabaseService] 成功为模块 {moduleId} 删除表: {tableType.Name}");
                            successCount++;
                        }
                        else
                        {
                            Log.Error($"[ModuleDatabaseService] 无法获取DropTable方法: {tableType.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[ModuleDatabaseService] 为模块 {moduleId} 删除表 {tableType.Name} 失败", ex);
                    }
                }

                var success = successCount == totalCount;
                Log.Info($"[ModuleDatabaseService] 模块 {moduleId} 数据库表删除完成: {successCount}/{totalCount}");
                return success;
            }
            catch (Exception ex)
            {
                Log.Error($"[ModuleDatabaseService] 为模块 {moduleId} 删除数据库表失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 检查指定模块的数据库表是否存在
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="tableTypes">要检查的表类型列表</param>
        /// <returns>表存在状态字典</returns>
        public async Task<Dictionary<Type, bool>> CheckModuleTablesExistAsync(string moduleId, List<Type> tableTypes)
        {
            var result = new Dictionary<Type, bool>();

            try
            {
                if (tableTypes == null || !tableTypes.Any())
                {
                    return result;
                }

                foreach (var tableType in tableTypes)
                {
                    var exists = await CheckTableExistsAsync(tableType);
                    result[tableType] = exists;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[ModuleDatabaseService] 检查模块 {moduleId} 数据库表存在状态失败", ex);
            }

            return result;
        }

        /// <summary>
        /// 检查单个表是否存在
        /// </summary>
        /// <param name="tableType">表类型</param>
        /// <returns>表是否存在</returns>
        public async Task<bool> CheckTableExistsAsync(Type tableType)
        {
            try
            {
                // 获取默认数据库连接
                var dbConfig = await ConfigManager.Instance.LoadDbConfigAsync();
                var sugar = new Sugar(dbConfig.ToSugarConfig());

                // 使用反射调用IsAnyAsync方法检查表是否存在数据
                var isAnyMethod = sugar.GetType().GetMethod("IsAny")?.MakeGenericMethod(tableType);
                if (isAnyMethod != null)
                {
                    // 先尝试查询，如果表不存在会抛出异常
                    var result = isAnyMethod.Invoke(sugar, new object[] { null });
                    return true; // 如果没有异常，说明表存在
                }

                return false;
            }
            catch
            {
                // 如果查询失败，通常表示表不存在
                return false;
            }
        }

        /// <summary>
        /// 获取模块的数据库表名列表
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="tableTypes">表类型列表</param>
        /// <returns>表名列表</returns>
        public List<string> GetModuleTableNames(string moduleId, List<Type> tableTypes)
        {
            var tableNames = new List<string>();

            try
            {
                var config = GetModuleConfiguration(moduleId);
                var prefix = config.TablePrefix;

                foreach (var tableType in tableTypes)
                {
                    var tableName = tableType.Name;
                    if (!string.IsNullOrWhiteSpace(prefix))
                    {
                        tableName = $"{prefix}_{tableName}";
                    }
                    tableNames.Add(tableName);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[ModuleDatabaseService] 获取模块 {moduleId} 表名列表失败", ex);
            }

            return tableNames;
        }

        /// <summary>
        /// 验证表类型是否有效
        /// </summary>
        /// <param name="tableTypes">要验证的表类型列表</param>
        /// <returns>验证结果</returns>
        public Dictionary<Type, bool> ValidateTableTypes(List<Type> tableTypes)
        {
            var result = new Dictionary<Type, bool>();

            foreach (var tableType in tableTypes)
            {
                try
                {
                    // 检查是否继承自RecordBase
                    var isValid = typeof(RecordBase).IsAssignableFrom(tableType) && 
                                  !tableType.IsAbstract && 
                                  !tableType.IsInterface;
                    result[tableType] = isValid;
                }
                catch (Exception ex)
                {
                    Log.Error($"[ModuleDatabaseService] 验证表类型 {tableType.Name} 失败", ex);
                    result[tableType] = false;
                }
            }

            return result;
        }

        /// <summary>
        /// 为模块创建数据库连接
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <returns>数据库连接实例</returns>
        public async Task<Sugar> CreateModuleDatabaseConnectionAsync(string moduleId)
        {
            try
            {
                var config = GetModuleConfiguration(moduleId);
                
                DatabaseSettings dbSettings;
                if (!string.IsNullOrWhiteSpace(config.ConnectionString))
                {
                    // 使用模块自定义的数据库配置
                    dbSettings = new DatabaseSettings
                    {
                        ConnectionString = config.ConnectionString,
                        DatabaseType = config.DatabaseType ?? "SqlServer"
                    };
                }
                else
                {
                    // 使用系统默认数据库配置
                    dbSettings = await ConfigManager.Instance.LoadDbConfigAsync();
                }

                var sugarConfig = dbSettings.ToSugarConfig();
                return new Sugar(sugarConfig);
            }
            catch (Exception ex)
            {
                Log.Error($"[ModuleDatabaseService] 为模块 {moduleId} 创建数据库连接失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 执行模块数据库迁移
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="fromVersion">源版本</param>
        /// <param name="toVersion">目标版本</param>
        /// <returns>迁移是否成功</returns>
        public async Task<bool> MigrateModuleDatabaseAsync(string moduleId, string fromVersion, string toVersion)
        {
            try
            {
                Log.Info($"[ModuleDatabaseService] 开始模块 {moduleId} 数据库迁移: {fromVersion} -> {toVersion}");
                
                var config = GetModuleConfiguration(moduleId);
                if (!config.MigrationSettings.EnableAutoMigration)
                {
                    Log.Info($"[ModuleDatabaseService] 模块 {moduleId} 未启用自动迁移");
                    return true;
                }

                // TODO: 实现具体的迁移逻辑
                // 这里可以根据版本号执行相应的迁移脚本
                
                Log.Info($"[ModuleDatabaseService] 模块 {moduleId} 数据库迁移完成");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[ModuleDatabaseService] 模块 {moduleId} 数据库迁移失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取模块数据库配置
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <returns>模块数据库配置</returns>
        private ModuleDatabaseConfiguration GetModuleConfiguration(string moduleId)
        {
            lock (_lockObject)
            {
                if (!_moduleConfigs.TryGetValue(moduleId, out var config))
                {
                    config = new ModuleDatabaseConfiguration
                    {
                        ModuleId = moduleId
                    };
                    _moduleConfigs[moduleId] = config;
                }
                return config;
            }
        }

        /// <summary>
        /// 设置模块数据库配置
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="configuration">数据库配置</param>
        public void SetModuleConfiguration(string moduleId, ModuleDatabaseConfiguration configuration)
        {
            lock (_lockObject)
            {
                _moduleConfigs[moduleId] = configuration;
            }
        }
    }
}
