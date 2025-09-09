using Logger;

namespace Core.Services
{
    /// <summary>
    /// 服务管理器
    /// 统一管理所有系统服务的生命周期
    /// </summary>
    public class ServiceManager : IDisposable
    {
        private readonly Dictionary<Type, object> _services;
        private bool _disposed = false;

        public ServiceManager()
        {
            _services = new Dictionary<Type, object>();
            Log.Info("服务管理器已启动");
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        public void RegisterService<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            var serviceType = typeof(T);
            _services[serviceType] = service;
            
            Log.Info($"服务已注册: {serviceType.Name}");
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        public T? GetService<T>() where T : class
        {
            var serviceType = typeof(T);
            if (_services.TryGetValue(serviceType, out var service))
            {
                return service as T;
            }
            return null;
        }

        /// <summary>
        /// 检查服务是否已注册
        /// </summary>
        public bool IsServiceRegistered<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 初始化所有核心服务
        /// </summary>
        public async Task InitializeServicesAsync()
        {
            try
            {
                Log.Info("开始初始化核心服务...");

                // 8. 模块管理器
                var moduleManager = new ModuleManager();
                RegisterService(moduleManager);

                // 9. 报警服务
                RegisterService(new AlarmService());
                
                Log.Info($"核心服务初始化完成，共注册 {_services.Count} 个服务");
            }
            catch (Exception ex)
            {
                Log.Error("初始化核心服务失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 启动所有服务
        /// </summary>
        public async Task StartServicesAsync()
        {
            try
            {
                Log.Info("开始启动服务...");

                // TODO

                Log.Info("服务启动完成");
            }
            catch (Exception ex)
            {
                Log.Error("启动服务失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 停止所有服务
        /// </summary>
        public async Task StopServicesAsync()
        {
            try
            {
                Log.Info("开始停止服务...");

                // 停止模块管理器
                var moduleManager = GetService<ModuleManager>();
                if (moduleManager != null)
                {
                    await moduleManager.ShutdownAllModulesAsync();
                }

                Log.Info("服务停止完成");
            }
            catch (Exception ex)
            {
                Log.Error("停止服务失败", ex);
            }
        }

        /// <summary>
        /// 获取服务状态
        /// </summary>
        public Dictionary<string, object> GetServiceStatus()
        {
            var status = new Dictionary<string, object>();

            try
            {
                status["ServiceCount"] = _services.Count;
                status["Services"] = _services.Keys.Select(t => t.Name).ToList();

                var systemMonitor = GetService<SystemMonitorService>();
                if (systemMonitor != null)
                {
                    var healthStatus = systemMonitor.CheckSystemHealth();
                    status["SystemHealth"] = healthStatus.OverallStatus;
                }

                var moduleManager = GetService<ModuleManager>();
                if (moduleManager != null)
                {
                    var modules = moduleManager.GetRegisteredModules();
                    status["ModuleCount"] = modules.Count;
                    status["RunningModules"] = modules.Count(m => m.IsRunning);
                }

            }
            catch (Exception ex)
            {
                Log.Error("获取服务状态失败", ex);
                status["Error"] = ex.Message;
            }

            return status;
        }

        /// <summary>
        /// 重启服务
        /// </summary>
        public async Task RestartServiceAsync<T>() where T : class
        {
            try
            {
                var serviceType = typeof(T);
                Log.Info($"重启服务: {serviceType.Name}");

                // 停止服务
                if (_services.TryGetValue(serviceType, out var service))
                {
                    if (service is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    _services.Remove(serviceType);
                }

                // 重新创建服务
                if (serviceType == typeof(SystemMonitorService))
                {
                    RegisterService(new SystemMonitorService());
                }
                // 可以根据需要添加其他服务的重启逻辑

                Log.Info($"服务重启完成: {serviceType.Name}");
            }
            catch (Exception ex)
            {
                Log.Error($"重启服务失败: {typeof(T).Name}", ex);
                throw;
            }
        }

        /// <summary>
        /// 获取服务诊断信息
        /// </summary>
        public Dictionary<string, object> GetServiceDiagnostics()
        {
            var diagnostics = new Dictionary<string, object>();

            try
            {
                // 基本信息
                diagnostics["Timestamp"] = DateTime.Now;
                diagnostics["ServiceCount"] = _services.Count;

                // 内存使用情况
                var process = System.Diagnostics.Process.GetCurrentProcess();
                diagnostics["ProcessMemoryMB"] = process.WorkingSet64 / 1024 / 1024;
                diagnostics["GCMemoryMB"] = GC.GetTotalMemory(false) / 1024 / 1024;

                // 系统监控信息
                var systemMonitor = GetService<SystemMonitorService>();
                if (systemMonitor != null)
                {
                    var performance = systemMonitor.GetCurrentPerformance();
                    diagnostics["SystemPerformance"] = new
                    {
                        CpuUsage = $"{performance.CpuUsage:F1}%",
                        MemoryUsage = $"{performance.MemoryUsagePercent:F1}%",
                        Uptime = performance.Uptime.ToString(@"dd\.hh\:mm\:ss")
                    };
                }

                // 模块状态
                var moduleManager = GetService<ModuleManager>();
                if (moduleManager != null)
                {
                    var modules = moduleManager.GetRegisteredModules();
                    diagnostics["ModuleStatus"] = modules.Select(m => new
                    {
                        Name = m.ModuleName,
                        Id = m.ModuleId,
                        IsInitialized = m.IsInitialized,
                        IsRunning = m.IsRunning
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error("获取服务诊断信息失败", ex);
                diagnostics["Error"] = ex.Message;
            }

            return diagnostics;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    StopServicesAsync().Wait(TimeSpan.FromSeconds(10));

                    // 释放所有服务
                    foreach (var service in _services.Values)
                    {
                        if (service is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }

                    _services.Clear();
                    _disposed = true;

                    Log.Info("服务管理器已释放");
                }
                catch (Exception ex)
                {
                    Log.Error("释放服务管理器失败", ex);
                }
            }
        }
    }
}
