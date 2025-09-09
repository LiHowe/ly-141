using System.IO;
using System.Reflection;
using System.Text.Json;
using Core.Interfaces;
using Core.Models;

namespace Core.Services;

/// <summary>
/// 动态模块加载器
/// 负责动态加载和管理外部模块
/// </summary>
public class DynamicModuleLoader : ServiceBase
{
    private readonly ModuleDiscoveryService _discoveryService;
    private readonly Dictionary<string, Assembly> _loadedAssemblies = new();
    private ModuleConfigurationRoot? _moduleConfig;

    public DynamicModuleLoader()
    {
        _discoveryService = new ModuleDiscoveryService();
    }

    /// <summary>
    /// 加载模块配置
    /// </summary>
    /// <returns>加载是否成功</returns>
    public async Task<bool> LoadModuleConfigurationAsync()
    {
        try
        {
            if (!File.Exists(Constants.ModuleConfigFilePath))
            {
                // 创建默认配置文件
                await CreateDefaultModuleConfigurationAsync();
            }

            var configJson = await File.ReadAllTextAsync(Constants.ModuleConfigFilePath);
            _moduleConfig = JsonSerializer.Deserialize<ModuleConfigurationRoot>(configJson);

            OnInfo($"模块配置加载成功，共 {_moduleConfig?.Modules.Count ?? 0} 个模块配置");
            return true;
        }
        catch (Exception ex)
        {
            OnError("加载模块配置失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 创建默认模块配置文件
    /// </summary>
    private async Task CreateDefaultModuleConfigurationAsync()
    {
        try
        {
            var defaultConfig = new ModuleConfigurationRoot
            {
                Version = "1.0",
                EnableDynamicLoading = true,
                LoadTimeoutSeconds = 30,
                Modules = new List<ModuleConfiguration>()
            };

            // 确保配置目录存在
            var configDir = Path.GetDirectoryName(Constants.ModuleConfigFilePath);
            if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            var configJson = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(Constants.ModuleConfigFilePath, configJson);
            OnInfo("创建默认模块配置文件");
        }
        catch (Exception ex)
        {
            OnError("创建默认模块配置文件失败", ex);
        }
    }

    /// <summary>
    /// 发现并加载所有动态模块
    /// </summary>
    /// <returns>加载的模块列表</returns>
    public async Task<List<IModule>> LoadDynamicModulesAsync()
    {
        var loadedModules = new List<IModule>();

        try
        {
            // 加载模块配置
            await LoadModuleConfigurationAsync();

            if (_moduleConfig?.EnableDynamicLoading != true)
            {
                OnInfo("动态模块加载已禁用");
                return loadedModules;
            }

            OnInfo("开始加载动态模块...");

            // 发现可用模块
            var discoveredModules = await _discoveryService.DiscoverModulesAsync();

            // 按优先级排序
            var sortedModules = discoveredModules
                .OrderBy(m => GetModulePriority(m.ModuleId))
                .ThenBy(m => m.SortOrder)
                .ToList();

            foreach (var discoveredModule in sortedModules)
            {
                try
                {
                    var moduleConfig = GetModuleConfiguration(discoveredModule.ModuleId);
                    
                    // 检查模块是否启用
                    if (moduleConfig?.IsEnabled == false)
                    {
                        OnInfo($"跳过已禁用的模块: {discoveredModule.ModuleName}");
                        continue;
                    }

                    // 加载模块
                    var module = await LoadModuleAsync(discoveredModule);
                    if (module != null)
                    {
                        loadedModules.Add(module);
                        OnInfo($"成功加载模块: {module.ModuleName} ({module.ModuleId})");
                    }
                }
                catch (Exception ex)
                {
                    OnError($"加载模块失败: {discoveredModule.ModuleName}", ex);
                }
            }

            OnInfo($"动态模块加载完成，共加载 {loadedModules.Count} 个模块");
            return loadedModules;
        }
        catch (Exception ex)
        {
            OnError("动态模块加载过程失败", ex);
            return loadedModules;
        }
    }

    /// <summary>
    /// 加载单个模块
    /// </summary>
    /// <param name="discoveryResult">模块发现结果</param>
    /// <returns>加载的模块实例</returns>
    private async Task<IModule?> LoadModuleAsync(ModuleDiscoveryResult discoveryResult)
    {
        try
        {
            OnInfo($"正在加载模块: {discoveryResult.ModuleName}");

            // 检查程序集是否已加载
            if (!_loadedAssemblies.ContainsKey(discoveryResult.AssemblyPath))
            {
                var assembly = Assembly.LoadFrom(discoveryResult.AssemblyPath);
                _loadedAssemblies[discoveryResult.AssemblyPath] = assembly;
                OnInfo($"加载程序集: {Path.GetFileName(discoveryResult.AssemblyPath)}");
            }

            // 创建模块实例
            var moduleInstance = discoveryResult.CreateInstance();
            if (moduleInstance == null)
            {
                OnError($"无法创建模块实例: {discoveryResult.ModuleId}");
                return null;
            }

            return moduleInstance;
        }
        catch (Exception ex)
        {
            OnError($"加载模块失败: {discoveryResult.ModuleId}", ex);
            return null;
        }
    }

    /// <summary>
    /// 获取模块配置
    /// </summary>
    /// <param name="moduleId">模块ID</param>
    /// <returns>模块配置</returns>
    private ModuleConfiguration? GetModuleConfiguration(string moduleId)
    {
        return _moduleConfig?.Modules.FirstOrDefault(m => m.ModuleId == moduleId);
    }

    /// <summary>
    /// 获取模块优先级
    /// </summary>
    /// <param name="moduleId">模块ID</param>
    /// <returns>优先级</returns>
    private int GetModulePriority(string moduleId)
    {
        var config = GetModuleConfiguration(moduleId);
        return config?.Priority ?? 100;
    }

    /// <summary>
    /// 保存模块配置
    /// </summary>
    /// <returns>保存是否成功</returns>
    public async Task<bool> SaveModuleConfigurationAsync()
    {
        try
        {
            if (_moduleConfig == null)
                return false;

            var configJson = JsonSerializer.Serialize(_moduleConfig, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(Constants.ModuleConfigFilePath, configJson);
            OnInfo("模块配置保存成功");
            return true;
        }
        catch (Exception ex)
        {
            OnError("保存模块配置失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 更新模块配置
    /// </summary>
    /// <param name="moduleId">模块ID</param>
    /// <param name="isEnabled">是否启用</param>
    /// <param name="priority">优先级</param>
    public void UpdateModuleConfiguration(string moduleId, bool isEnabled, int? priority = null)
    {
        if (_moduleConfig == null)
            return;

        var existingConfig = _moduleConfig.Modules.FirstOrDefault(m => m.ModuleId == moduleId);
        if (existingConfig != null)
        {
            existingConfig.IsEnabled = isEnabled;
            if (priority.HasValue)
                existingConfig.Priority = priority.Value;
        }
        else
        {
            _moduleConfig.Modules.Add(new ModuleConfiguration
            {
                ModuleId = moduleId,
                IsEnabled = isEnabled,
                Priority = priority ?? 100
            });
        }
    }

    /// <summary>
    /// 获取已加载的程序集
    /// </summary>
    /// <returns>已加载的程序集字典</returns>
    public Dictionary<string, Assembly> GetLoadedAssemblies()
    {
        return new Dictionary<string, Assembly>(_loadedAssemblies);
    }


}
