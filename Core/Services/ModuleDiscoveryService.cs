using System.IO;
using System.Reflection;
using Core.Interfaces;

namespace Core.Services;

/// <summary>
/// 模块发现服务
/// 负责扫描和发现可用的模块
/// </summary>
public class ModuleDiscoveryService : ServiceBase
{
    private readonly List<ModuleDiscoveryResult> _discoveredModules = new();

    public ModuleDiscoveryService()
    {
    }

    /// <summary>
    /// 扫描并发现所有可用模块
    /// </summary>
    /// <returns>发现的模块列表</returns>
    public async Task<List<ModuleDiscoveryResult>> DiscoverModulesAsync()
    {
        _discoveredModules.Clear();

        try
        {
            OnInfo("开始扫描模块...");

            // 确保模块目录存在
            if (!Directory.Exists(Constants.ModulesRootPath))
            {
                Directory.CreateDirectory(Constants.ModulesRootPath);
                OnInfo($"创建模块目录: {Constants.ModulesRootPath}");
            }

            // 扫描模块目录中的所有 DLL 文件
            var dllFiles = Directory.GetFiles(Constants.ModulesRootPath, "*.dll", SearchOption.AllDirectories);
            OnInfo($"发现 {dllFiles.Length} 个 DLL 文件");

            foreach (var dllFile in dllFiles)
            {
                await DiscoverModulesInAssemblyAsync(dllFile);
            }

            OnInfo($"模块扫描完成，共发现 {_discoveredModules.Count} 个模块");
            return _discoveredModules.ToList();
        }
        catch (Exception ex)
        {
            OnError("模块扫描失败", ex);
            return new List<ModuleDiscoveryResult>();
        }
    }

    /// <summary>
    /// 在指定程序集中发现模块
    /// </summary>
    /// <param name="assemblyPath">程序集路径</param>
    private async Task DiscoverModulesInAssemblyAsync(string assemblyPath)
    {
        try
        {
            OnInfo($"正在扫描程序集: {Path.GetFileName(assemblyPath)}");

            // 加载程序集
            var assembly = Assembly.LoadFrom(assemblyPath);
            
            // 查找实现 IModule 接口的类型
            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t) && 
                           !t.IsInterface && 
                           !t.IsAbstract)
                .ToList();

            foreach (var moduleType in moduleTypes)
            {
                try
                {
                    var discoveryResult = await CreateModuleDiscoveryResultAsync(moduleType, assemblyPath);
                    if (discoveryResult != null)
                    {
                        _discoveredModules.Add(discoveryResult);
                        OnInfo($"发现模块: {discoveryResult.ModuleName} ({discoveryResult.ModuleId})");
                    }
                }
                catch (Exception ex)
                {
                    OnError($"创建模块发现结果失败: {moduleType.FullName}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            OnError($"扫描程序集失败: {assemblyPath}", ex);
        }
    }

    /// <summary>
    /// 创建模块发现结果
    /// </summary>
    /// <param name="moduleType">模块类型</param>
    /// <param name="assemblyPath">程序集路径</param>
    /// <returns>模块发现结果</returns>
    private async Task<ModuleDiscoveryResult?> CreateModuleDiscoveryResultAsync(Type moduleType, string assemblyPath)
    {
        try
        {
            // 尝试创建模块实例以获取元数据
            var moduleInstance = Activator.CreateInstance(moduleType) as IModule;
            if (moduleInstance == null)
            {
                OnError($"无法创建模块实例: {moduleType.FullName}");
                return null;
            }

            var result = new ModuleDiscoveryResult
            {
                ModuleType = moduleType,
                AssemblyPath = assemblyPath,
                ModuleId = moduleInstance.ModuleId,
                ModuleName = moduleInstance.ModuleName,
                Description = moduleInstance.Description,
                Version = moduleInstance.Version,
                IconPath = moduleInstance.IconPath,
                SortOrder = moduleInstance.SortOrder,
                IsEnabled = moduleInstance.IsEnabled
            };

            return result;
        }
        catch (Exception ex)
        {
            OnError($"创建模块发现结果失败: {moduleType.FullName}", ex);
            return null;
        }
    }

    /// <summary>
    /// 根据模块ID获取发现的模块
    /// </summary>
    /// <param name="moduleId">模块ID</param>
    /// <returns>模块发现结果</returns>
    public ModuleDiscoveryResult? GetDiscoveredModule(string moduleId)
    {
        return _discoveredModules.FirstOrDefault(m => m.ModuleId == moduleId);
    }

    /// <summary>
    /// 获取所有发现的模块
    /// </summary>
    /// <returns>模块发现结果列表</returns>
    public List<ModuleDiscoveryResult> GetDiscoveredModules()
    {
        return _discoveredModules.ToList();
    }


}

/// <summary>
/// 模块发现结果
/// </summary>
public class ModuleDiscoveryResult
{
    /// <summary>
    /// 模块类型
    /// </summary>
    public Type ModuleType { get; set; } = null!;

    /// <summary>
    /// 程序集路径
    /// </summary>
    public string AssemblyPath { get; set; } = string.Empty;

    /// <summary>
    /// 模块ID
    /// </summary>
    public string ModuleId { get; set; } = string.Empty;

    /// <summary>
    /// 模块名称
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// 模块描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 模块版本
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 模块图标路径
    /// </summary>
    public string? IconPath { get; set; }

    /// <summary>
    /// 排序权重
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 创建模块实例
    /// </summary>
    /// <returns>模块实例</returns>
    public IModule? CreateInstance()
    {
        try
        {
            return Activator.CreateInstance(ModuleType) as IModule;
        }
        catch
        {
            return null;
        }
    }
}
