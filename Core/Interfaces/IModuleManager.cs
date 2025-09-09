using System.Collections.ObjectModel;

namespace Core.Interfaces
{
    /// <summary>
    /// 模块管理器接口
    /// 负责模块的注册、加载、启用/禁用等管理功能
    /// </summary>
    public interface IModuleManager
    {
        /// <summary>
        /// 所有已注册的模块
        /// </summary>
        ReadOnlyCollection<IModule> Modules { get; }

        /// <summary>
        /// 已启用的模块
        /// </summary>
        ReadOnlyCollection<IModule> EnabledModules { get; }

        /// <summary>
        /// 模块状态变化事件
        /// </summary>
        event EventHandler<ModuleStatusChangedEventArgs>? ModuleStatusChanged;

        /// <summary>
        /// 注册模块
        /// </summary>
        /// <param name="module">要注册的模块</param>
        /// <returns>注册是否成功</returns>
        Task<bool> RegisterModuleAsync(IModule module);

        /// <summary>
        /// 注册多个模块
        /// </summary>
        /// <param name="modules">要注册的模块集合</param>
        /// <returns>成功注册的模块数量</returns>
        Task<int> RegisterModulesAsync(IEnumerable<IModule> modules);

        /// <summary>
        /// 取消注册模块
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <returns>取消注册是否成功</returns>
        Task<bool> UnregisterModuleAsync(string moduleId);

        /// <summary>
        /// 根据ID获取模块
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <returns>模块实例，如果不存在则返回null</returns>
        IModule? GetModule(string moduleId);

        /// <summary>
        /// 启用模块
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <returns>启用是否成功</returns>
        Task<bool> EnableModuleAsync(string moduleId);

        /// <summary>
        /// 禁用模块
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <returns>禁用是否成功</returns>
        Task<bool> DisableModuleAsync(string moduleId);

        /// <summary>
        /// 初始化所有模块
        /// </summary>
        /// <returns>初始化结果</returns>
        Task<ModuleInitializationResult> InitializeAllModulesAsync();

        /// <summary>
        /// 启动所有已启用的模块
        /// </summary>
        /// <returns>启动结果</returns>
        Task<ModuleStartupResult> StartupAllModulesAsync();

        /// <summary>
        /// 关闭所有模块
        /// </summary>
        /// <returns>关闭结果</returns>
        Task<ModuleShutdownResult> ShutdownAllModulesAsync();
    }

    /// <summary>
    /// 模块状态变化事件参数
    /// </summary>
    public class ModuleStatusChangedEventArgs : EventArgs
    {
        public string ModuleId { get; }
        public ModuleStatus OldStatus { get; }
        public ModuleStatus NewStatus { get; }

        public ModuleStatusChangedEventArgs(string moduleId, ModuleStatus oldStatus, ModuleStatus newStatus)
        {
            ModuleId = moduleId;
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }

    /// <summary>
    /// 模块状态枚举
    /// </summary>
    public enum ModuleStatus
    {
        /// <summary>
        /// 未注册
        /// </summary>
        Unregistered,

        /// <summary>
        /// 已注册但未初始化
        /// </summary>
        Registered,

        /// <summary>
        /// 已初始化但未启动
        /// </summary>
        Initialized,

        /// <summary>
        /// 已启动
        /// </summary>
        Started,

        /// <summary>
        /// 已禁用
        /// </summary>
        Disabled,

        /// <summary>
        /// 错误状态
        /// </summary>
        Error
    }

    /// <summary>
    /// 模块初始化结果
    /// </summary>
    public class ModuleInitializationResult
    {
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> FailedModules { get; set; } = new();
        public List<Exception> Exceptions { get; set; } = new();
    }

    /// <summary>
    /// 模块启动结果
    /// </summary>
    public class ModuleStartupResult
    {
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> FailedModules { get; set; } = new();
        public List<Exception> Exceptions { get; set; } = new();
    }

    /// <summary>
    /// 模块关闭结果
    /// </summary>
    public class ModuleShutdownResult
    {
        public int TotalCount { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> FailedModules { get; set; } = new();
        public List<Exception> Exceptions { get; set; } = new();
    }
}
