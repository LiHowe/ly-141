using Core.Models;
using Core.Models.Records;
using Data.SqlSugar;

namespace Core.Interfaces
{
    /// <summary>
    /// 模块数据库服务接口
    /// 提供模块级别的数据库表管理功能
    /// </summary>
    public interface IModuleDatabaseService
    {
        /// <summary>
        /// 为指定模块创建数据库表
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="tableTypes">要创建的表类型列表</param>
        /// <returns>创建是否成功</returns>
        Task<bool> CreateModuleTablesAsync(string moduleId, List<Type> tableTypes);

        /// <summary>
        /// 删除指定模块的数据库表
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="tableTypes">要删除的表类型列表</param>
        /// <returns>删除是否成功</returns>
        Task<bool> DropModuleTablesAsync(string moduleId, List<Type> tableTypes);

        /// <summary>
        /// 检查指定模块的数据库表是否存在
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="tableTypes">要检查的表类型列表</param>
        /// <returns>表存在状态字典，Key为表类型，Value为是否存在</returns>
        Task<Dictionary<Type, bool>> CheckModuleTablesExistAsync(string moduleId, List<Type> tableTypes);

        /// <summary>
        /// 检查单个表是否存在
        /// </summary>
        /// <param name="tableType">表类型</param>
        /// <returns>表是否存在</returns>
        Task<bool> CheckTableExistsAsync(Type tableType);

        /// <summary>
        /// 获取模块的数据库表名列表
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="tableTypes">表类型列表</param>
        /// <returns>表名列表</returns>
        List<string> GetModuleTableNames(string moduleId, List<Type> tableTypes);

        /// <summary>
        /// 验证表类型是否有效（继承自RecordBase）
        /// </summary>
        /// <param name="tableTypes">要验证的表类型列表</param>
        /// <returns>验证结果，Key为表类型，Value为是否有效</returns>
        Dictionary<Type, bool> ValidateTableTypes(List<Type> tableTypes);

        /// <summary>
        /// 为模块创建数据库连接
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <returns>数据库连接实例</returns>
        Task<Sugar> CreateModuleDatabaseConnectionAsync(string moduleId);

        /// <summary>
        /// 执行模块数据库迁移
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="fromVersion">源版本</param>
        /// <param name="toVersion">目标版本</param>
        /// <returns>迁移是否成功</returns>
        Task<bool> MigrateModuleDatabaseAsync(string moduleId, string fromVersion, string toVersion);
    }

    /// <summary>
    /// 模块数据库操作结果
    /// </summary>
    public class ModuleDatabaseOperationResult
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 操作消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// 受影响的表数量
        /// </summary>
        public int AffectedTableCount { get; set; }

        /// <summary>
        /// 操作详情
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new();
    }


}
