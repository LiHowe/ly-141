using System.Collections.ObjectModel;
using Core.Interfaces;
using Logger;

namespace Core.Services
{
    /// <summary>
    /// 模块管理器实现
    /// 负责模块的注册、加载、启用/禁用等管理功能
    /// </summary>
    public class ModuleManager : IModuleManager
    {
        private readonly List<IModule> _modules = new();
        private readonly Dictionary<string, ModuleStatus> _moduleStatuses = new();
        private readonly IModuleDatabaseService _databaseService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="databaseService">模块数据库服务，如果为null则创建默认实例</param>
        public ModuleManager(IModuleDatabaseService? databaseService = null)
        {
            _databaseService = databaseService ?? new ModuleDatabaseService();
        }

        /// <summary>
        /// 所有已注册的模块
        /// </summary>
        public ReadOnlyCollection<IModule> Modules => _modules.AsReadOnly();

        /// <summary>
        /// 已启用的模块
        /// </summary>
        public ReadOnlyCollection<IModule> EnabledModules => 
            _modules.Where(m => m.IsEnabled).ToList().AsReadOnly();

        /// <summary>
        /// 模块状态变化事件
        /// </summary>
        public event EventHandler<ModuleStatusChangedEventArgs>? ModuleStatusChanged;

        /// <summary>
        /// 注册模块
        /// </summary>
        /// <param name="module">要注册的模块</param>
        /// <returns>注册是否成功</returns>
        public async Task<bool> RegisterModuleAsync(IModule module)
        {
            try
            {
                if (module == null)
                    return false;

                // 检查是否已经注册
                if (_modules.Any(m => m.ModuleId == module.ModuleId))
                {
                    OnError($"模块 {module.ModuleId} 已经注册");
                    return false;
                }

                _modules.Add(module);
                _moduleStatuses[module.ModuleId] = ModuleStatus.Registered;

                OnModuleStatusChanged(module.ModuleId, ModuleStatus.Unregistered, ModuleStatus.Registered);

                OnInfo($"模块 {module.ModuleName} ({module.ModuleId}) 注册成功");
                return true;
            }
            catch (Exception ex)
            {
                OnError($"注册模块 {module?.ModuleId} 失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 注册多个模块
        /// </summary>
        /// <param name="modules">要注册的模块集合</param>
        /// <returns>成功注册的模块数量</returns>
        public async Task<int> RegisterModulesAsync(IEnumerable<IModule> modules)
        {
            int successCount = 0;
            foreach (var module in modules)
            {
                if (await RegisterModuleAsync(module))
                    successCount++;
            }
            return successCount;
        }

        /// <summary>
        /// 取消注册模块
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <returns>取消注册是否成功</returns>
        public async Task<bool> UnregisterModuleAsync(string moduleId)
        {
            try
            {
                var module = GetModule(moduleId);
                if (module == null)
                    return false;

                // 先关闭模块
                await module.OnShutdownAsync();

                // 清理模块数据库资源
                await CleanupModuleDatabaseAsync(module);

                _modules.Remove(module);
                _moduleStatuses.Remove(moduleId);

                OnModuleStatusChanged(moduleId, ModuleStatus.Registered, ModuleStatus.Unregistered);

                OnInfo($"模块 {module.ModuleName} ({moduleId}) 取消注册成功");
                return true;
            }
            catch (Exception ex)
            {
                OnError($"取消注册模块 {moduleId} 失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 根据ID获取模块
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <returns>模块实例，如果不存在则返回null</returns>
        public IModule? GetModule(string moduleId)
        {
            return _modules.FirstOrDefault(m => m.ModuleId == moduleId);
        }

        /// <summary>
        /// 获取所有已注册的模块（按排序顺序）
        /// </summary>
        /// <returns>已注册的模块列表</returns>
        public List<IModule> GetRegisteredModules()
        {
            return _modules.OrderBy(m => m.SortOrder).ToList();
        }

        /// <summary>
        /// 启用模块
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <returns>启用是否成功</returns>
        public async Task<bool> EnableModuleAsync(string moduleId)
        {
            try
            {
                var module = GetModule(moduleId);
                if (module == null)
                    return false;

                if (module.IsEnabled)
                    return true;

                // 启用
                await module.OnEnabledAsync();
                // 启动
                await module.OnStartupAsync();
                OnModuleStatusChanged(moduleId, ModuleStatus.Disabled, ModuleStatus.Started);
                OnInfo($"模块 {module.ModuleName} ({moduleId}) 启用成功");
                module.IsEnabled = true;
                return true;
            }
            catch (Exception ex)
            {
                OnError($"启用模块 {moduleId} 失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 禁用模块
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <returns>禁用是否成功</returns>
        public async Task<bool> DisableModuleAsync(string moduleId)
        {
            try
            {
                var module = GetModule(moduleId);
                if (module == null)
                    return false;

                if (!module.IsEnabled)
                    return true;

                await module.OnDisabledAsync();
                OnModuleStatusChanged(moduleId, ModuleStatus.Started, ModuleStatus.Disabled);
                OnInfo($"模块 {module.ModuleName} ({moduleId}) 禁用成功");
                module.IsEnabled = false;
                return true;
            }
            catch (Exception ex)
            {
                OnError($"禁用模块 {moduleId} 失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 初始化所有模块
        /// </summary>
        /// <returns>初始化结果</returns>
        public async Task<ModuleInitializationResult> InitializeAllModulesAsync()
        {
            var result = new ModuleInitializationResult
            {
                TotalCount = _modules.Count
            };

            foreach (var module in _modules)
            {
                try
                {
                    // 1. 首先初始化模块本身
                    var moduleInitSuccess = await module.InitializeAsync();
                    if (!moduleInitSuccess)
                    {
                        result.FailedCount++;
                        result.FailedModules.Add(module.ModuleId);
                        _moduleStatuses[module.ModuleId] = ModuleStatus.Error;
                        OnModuleStatusChanged(module.ModuleId, ModuleStatus.Registered, ModuleStatus.Error);
                        continue;
                    }

                    // 2. 初始化模块数据库表
                    var dbInitSuccess = await InitializeModuleDatabaseAsync(module);
                    if (!dbInitSuccess)
                    {
                        OnError($"模块 {module.ModuleId} 数据库初始化失败，但模块初始化成功");
                        // 数据库初始化失败不影响模块的基本功能，继续标记为已初始化
                    }

                    result.SuccessCount++;
                    _moduleStatuses[module.ModuleId] = ModuleStatus.Initialized;
                    OnModuleStatusChanged(module.ModuleId, ModuleStatus.Registered, ModuleStatus.Initialized);
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.FailedModules.Add(module.ModuleId);
                    result.Exceptions.Add(ex);
                    _moduleStatuses[module.ModuleId] = ModuleStatus.Error;
                    OnModuleStatusChanged(module.ModuleId, ModuleStatus.Registered, ModuleStatus.Error);
                    OnError($"初始化模块 {module.ModuleId} 失败", ex);
                }
            }

            OnInfo($"模块初始化完成: 总数 {result.TotalCount}, 成功 {result.SuccessCount}, 失败 {result.FailedCount}");
            return result;
        }

        /// <summary>
        /// 启动所有已启用的模块
        /// </summary>
        /// <returns>启动结果</returns>
        public async Task<ModuleStartupResult> StartupAllModulesAsync()
        {
            var enabledModules = _modules.Where(m => m.IsEnabled).ToList();
            var result = new ModuleStartupResult
            {
                TotalCount = enabledModules.Count
            };

            foreach (var module in enabledModules)
            {
                try
                {
                    await module.OnStartupAsync();
                    result.SuccessCount++;
                    _moduleStatuses[module.ModuleId] = ModuleStatus.Started;
                    OnModuleStatusChanged(module.ModuleId, ModuleStatus.Initialized, ModuleStatus.Started);
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.FailedModules.Add(module.ModuleId);
                    result.Exceptions.Add(ex);
                    _moduleStatuses[module.ModuleId] = ModuleStatus.Error;
                    OnModuleStatusChanged(module.ModuleId, ModuleStatus.Initialized, ModuleStatus.Error);
                    OnError($"启动模块 {module.ModuleId} 失败", ex);
                }
            }

            OnInfo($"模块启动完成: 总数 {result.TotalCount}, 成功 {result.SuccessCount}, 失败 {result.FailedCount}");
            return result;
        }

        /// <summary>
        /// 关闭所有模块
        /// </summary>
        /// <returns>关闭结果</returns>
        public async Task<ModuleShutdownResult> ShutdownAllModulesAsync()
        {
            var result = new ModuleShutdownResult
            {
                TotalCount = _modules.Count
            };

            // 按照启动顺序的逆序关闭模块
            var modulesToShutdown = _modules.OrderByDescending(m => m.SortOrder).ToList();

            foreach (var module in modulesToShutdown)
            {
                try
                {
                    await module.OnShutdownAsync();
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.FailedModules.Add(module.ModuleId);
                    result.Exceptions.Add(ex);
                    OnError($"关闭模块 {module.ModuleId} 失败", ex);
                }
            }

            OnInfo($"模块关闭完成: 总数 {result.TotalCount}, 成功 {result.SuccessCount}, 失败 {result.FailedCount}");
            return result;
        }

        /// <summary>
        /// 触发模块状态变化事件
        /// </summary>
        private void OnModuleStatusChanged(string moduleId, ModuleStatus oldStatus, ModuleStatus newStatus)
        {
            ModuleStatusChanged?.Invoke(this, new ModuleStatusChangedEventArgs(moduleId, oldStatus, newStatus));
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        private void OnInfo(string message)
        {
            Log.Info($"[ModuleManager] {message}");
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        private void OnError(string message, Exception? exception = null)
        {
            var fullMessage = exception != null ? $"{message}: {exception.Message}" : message;
            Log.Error($"[ModuleManager] ERROR: {fullMessage}", exception);
        }

        #region 数据库相关方法

        /// <summary>
        /// 初始化模块数据库
        /// </summary>
        /// <param name="module">要初始化数据库的模块</param>
        /// <returns>初始化是否成功</returns>
        private async Task<bool> InitializeModuleDatabaseAsync(IModule module)
        {
            try
            {
                // 获取模块需要的数据库表类型
                var tableTypes = module.GetDatabaseTableTypes();
                if (tableTypes == null || !tableTypes.Any())
                {
                    OnInfo($"模块 {module.ModuleId} 不需要数据库表，跳过数据库初始化");
                    return true;
                }

                OnInfo($"开始为模块 {module.ModuleId} 初始化数据库，表数量: {tableTypes.Count}");

                // 创建数据库表
                var createSuccess = await _databaseService.CreateModuleTablesAsync(module.ModuleId, tableTypes);
                if (!createSuccess)
                {
                    OnError($"为模块 {module.ModuleId} 创建数据库表失败");
                    return false;
                }

                // 调用模块的数据库初始化方法
                var moduleDbInitSuccess = await module.OnDatabaseInitializeAsync();
                if (!moduleDbInitSuccess)
                {
                    OnError($"模块 {module.ModuleId} 数据库初始化回调失败");
                    return false;
                }

                OnInfo($"模块 {module.ModuleId} 数据库初始化完成");
                return true;
            }
            catch (Exception ex)
            {
                OnError($"初始化模块 {module.ModuleId} 数据库失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 清理模块数据库资源
        /// </summary>
        /// <param name="module">要清理数据库资源的模块</param>
        /// <returns>清理是否成功</returns>
        private async Task<bool> CleanupModuleDatabaseAsync(IModule module)
        {
            try
            {
                // 获取模块需要的数据库表类型
                var tableTypes = module.GetDatabaseTableTypes();
                if (tableTypes == null || !tableTypes.Any())
                {
                    OnInfo($"模块 {module.ModuleId} 不需要数据库表，跳过数据库清理");
                    return true;
                }

                OnInfo($"开始为模块 {module.ModuleId} 清理数据库资源");

                // 删除数据库表（根据配置决定是否删除）
                var dropSuccess = await _databaseService.DropModuleTablesAsync(module.ModuleId, tableTypes);
                if (!dropSuccess)
                {
                    OnError($"为模块 {module.ModuleId} 删除数据库表失败");
                    return false;
                }

                OnInfo($"模块 {module.ModuleId} 数据库资源清理完成");
                return true;
            }
            catch (Exception ex)
            {
                OnError($"清理模块 {module.ModuleId} 数据库资源失败", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取模块数据库服务
        /// </summary>
        /// <returns>模块数据库服务实例</returns>
        public IModuleDatabaseService GetDatabaseService()
        {
            return _databaseService;
        }

        #endregion
    }
}
