using System.Windows.Controls;
using Core.Interfaces;
using Core.Models.Settings;
using Core.Utils;
using Data.SqlSugar;
using Logger;

namespace Core.Models;

/// <summary>
/// 模块基类
/// 提供模块的基础实现，所有模块都应该继承此类
/// </summary>
public abstract class ModuleBase : IModule
{
    /// <summary>
    /// 模块唯一标识符
    /// </summary>
    public abstract string ModuleId { get; }

    /// <summary>
    /// 模块显示名称
    /// </summary>
    public abstract string ModuleName { get; }

    /// <summary>
    /// 模块描述
    /// </summary>
    public virtual string Description => string.Empty;

    /// <summary>
    /// 模块版本
    /// </summary>
    public virtual string Version => "1.0.0";

    /// <summary>
    /// 模块是否已启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 模块图标路径
    /// </summary>
    public virtual string? IconPath => null;

    /// <summary>
    /// 模块排序权重（用于菜单排序）
    /// </summary>
    public virtual int SortOrder => 0;

    /// <summary>
    /// 模块是否已初始化
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// 模块是否正在运行
    /// </summary>
    public bool IsRunning => IsStarted;

    /// <summary>
    /// 模块是否已启动
    /// </summary>
    protected bool IsStarted { get; private set; }

    /// <summary>
    /// 同步锁对象，用于保证线程安全
    /// </summary>
    private readonly object _syncLock = new object();

    /// <summary>
    /// 模块数据库连接实例
    /// </summary>
    protected Sugar? DatabaseConnection { get; private set; }

    /// <summary>
    /// 模块是否已完成数据库初始化
    /// </summary>
    public bool IsDatabaseInitialized { get; private set; }

    /// <summary>
    /// 初始化模块
    /// </summary>
    /// <returns>初始化是否成功</returns>
    public virtual async Task<bool> InitializeAsync()
    {
        // 使用锁确保线程安全
        lock (_syncLock)
        {
            if (IsInitialized)
                return true;
        }

        try
        {
            await OnInitializeAsync();

            lock (_syncLock)
            {
                IsInitialized = true;
            }

            return true;
        }
        catch (Exception ex)
        {
            OnError($"模块 {ModuleName} 初始化失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 根据页面名称创建页面实例
    /// </summary>
    /// <param name="pageName">页面名称</param>
    /// <returns>页面实例，如果页面不存在则返回null</returns>
    public virtual UserControl? CreatePage(string pageName)
    {
        if (string.IsNullOrWhiteSpace(pageName))
        {
            OnError($"模块 {ModuleName} 创建页面失败：页面名称不能为空", new ArgumentException("页面名称不能为空"));
            return null;
        }

        try
        {
            // 尝试通过反射创建页面
            var pageType = GetPageType(pageName);
            if (pageType != null)
            {
                return Activator.CreateInstance(pageType) as UserControl;
            }

            OnError($"模块 {ModuleName} 中未找到页面: {pageName}", new ArgumentException($"Page not found: {pageName}"));
            return null;
        }
        catch (Exception ex)
        {
            OnError($"创建模块 {ModuleName} 页面 {pageName} 失败", ex);
            return null;
        }
    }

    /// <summary>
    /// 根据页面名称获取页面类型
    /// 子类可以重写此方法来提供自定义的页面类型解析逻辑
    /// </summary>
    /// <param name="pageName">页面名称</param>
    /// <returns>页面类型，如果未找到则返回null</returns>
    protected virtual Type? GetPageType(string pageName)
    {
        try
        {
            // 首先尝试在当前模块程序集中查找
            var moduleAssembly = GetType().Assembly;
            var fullTypeName = $"{GetType().Namespace}.{pageName}";
            var pageType = moduleAssembly.GetType(fullTypeName);

            if (pageType != null && typeof(UserControl).IsAssignableFrom(pageType))
            {
                return pageType;
            }

            // 如果在当前命名空间找不到，尝试在整个程序集中搜索
            var types = moduleAssembly.GetTypes()
                .Where(t => typeof(UserControl).IsAssignableFrom(t) &&
                            (t.Name == pageName || t.FullName?.EndsWith($".{pageName}") == true))
                .ToArray();

            if (types.Length == 1)
            {
                return types[0];
            }
            else if (types.Length > 1)
            {
                // 如果找到多个匹配的类型，优先选择名称完全匹配的
                var exactMatch = types.FirstOrDefault(t => t.Name == pageName);
                if (exactMatch != null)
                {
                    return exactMatch;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            OnError($"获取页面类型失败: {pageName}", ex);
            return null;
        }
    }

    /// <summary>
    /// 模块启动时调用
    /// </summary>
    public virtual async Task OnStartupAsync()
    {
        try
        {
            // 检查是否已启动
            if (IsStarted)
                return;

            // 检查是否已初始化，未初始化则先初始化
            if (!IsInitialized)
            {
                var initResult = await InitializeAsync();
                if (!initResult)
                {
                    OnError("模块未能成功初始化，启动失败", new InvalidOperationException("模块初始化失败"));
                    return;
                }
            }

            // 检查模块是否已启用
            if (!IsEnabled)
            {
                OnError($"模块 {ModuleName} 未启用，无法启动", new InvalidOperationException("模块未启用"));
                return;
            }

            // 执行具体的启动逻辑
            await OnStartAsync();

            // 标记为已启动
            lock (_syncLock)
            {
                IsStarted = true;
            }
        }
        catch (Exception ex)
        {
            OnError($"模块 {ModuleName} 启动失败", ex);
        }
    }

    /// <summary>
    /// 模块关闭时调用
    /// </summary>
    public virtual async Task OnShutdownAsync()
    {
        try
        {
            if (!IsStarted)
                return;

            await OnStopAsync();

            lock (_syncLock)
            {
                IsStarted = false;
            }
        }
        catch (Exception ex)
        {
            OnError($"模块 {ModuleName} 关闭失败", ex);
        }
    }

    /// <summary>
    /// 模块启用时调用
    /// </summary>
    public virtual async Task OnEnabledAsync()
    {
        try
        {
            lock (_syncLock)
            {
                IsEnabled = true;
            }

            await OnEnableAsync();
        }
        catch (Exception ex)
        {
            OnError($"模块 {ModuleName} 启用失败", ex);
        }
    }

    /// <summary>
    /// 模块禁用时调用
    /// </summary>
    public virtual async Task OnDisabledAsync()
    {
        try
        {
            // 如果模块正在运行，先停止它
            if (IsStarted)
            {
                await OnShutdownAsync();
            }

            lock (_syncLock)
            {
                IsEnabled = false;
            }

            await OnDisableAsync();
        }
        catch (Exception ex)
        {
            OnError($"模块 {ModuleName} 禁用失败", ex);
        }
    }

    /// <summary>
    /// 子类重写此方法实现具体的初始化逻辑
    /// </summary>
    protected virtual Task OnInitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 子类重写此方法实现具体的启动逻辑
    /// </summary>
    protected virtual Task OnStartAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 子类重写此方法实现具体的停止逻辑
    /// </summary>
    protected virtual Task OnStopAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 子类重写此方法实现具体的启用逻辑
    /// </summary>
    protected virtual Task OnEnableAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 子类重写此方法实现具体的禁用逻辑
    /// </summary>
    protected virtual Task OnDisableAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 错误处理方法
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="exception">异常对象</param>
    protected virtual void OnError(string message, Exception? exception = null)
    {
        // 这里可以集成日志系统
        Log.Error($"[{ModuleName}] {message}", exception);

        // 可以考虑添加事件通知机制
        // ErrorOccurred?.Invoke(this, new ModuleErrorEventArgs(message, exception));
    }
    
    /// <summary>
    /// 信息处理方法
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="exception">异常对象</param>
    protected virtual void OnInfo(string message)
    {
        // 这里可以集成日志系统
        Log.Info($"[{ModuleName}] {message}");
    }

    /// <summary>
    /// 重写ToString方法
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{ModuleName} ({ModuleId}) - v{Version}";
    }

    /// <summary>
    /// 获取模块菜单配置
    /// </summary>
    /// <returns>模块的菜单配置列表</returns>
    public virtual List<MenuConfiguration> GetMenuConfigurations()
    {
        return new List<MenuConfiguration>();
    }

    /// <summary>
    /// 获取模块设置页面
    /// </summary>
    /// <returns>设置页面实例，如果没有设置页面则返回null</returns>
    public virtual UserControl? GetSettingsPage()
    {
        var pageType = GetSettingsPageType();
        if (pageType == null)
            return null;

        try
        {
            return Activator.CreateInstance(pageType) as UserControl;
        }
        catch (Exception ex)
        {
            OnError($"创建模块 {ModuleName} 设置页面失败", ex);
            return null;
        }
    }

    /// <summary>
    /// 获取模块设置页面类型
    /// </summary>
    /// <returns>设置页面类型，如果没有设置页面则返回null</returns>
    public virtual Type? GetSettingsPageType()
    {
        return null;
    }

    /// <summary>
    /// 是否有设置页面
    /// </summary>
    public virtual bool HasSettingsPage => GetSettingsPageType() != null;

    #region 数据库相关方法

    /// <summary>
    /// 获取模块需要的数据库表类型
    /// 子类可以重写此方法返回模块特定的数据表类型
    /// </summary>
    /// <returns>数据库表类型列表</returns>
    public virtual List<Type> GetDatabaseTableTypes()
    {
        return new List<Type>();
    }

    /// <summary>
    /// 模块数据库初始化时调用
    /// 在模块初始化过程中，数据库表创建完成后会调用此方法
    /// 子类可以重写此方法执行特定的数据库初始化逻辑
    /// </summary>
    /// <returns>数据库初始化是否成功</returns>
    public virtual async Task<bool> OnDatabaseInitializeAsync()
    {
        try
        {
            // 创建数据库连接
            await InitializeDatabaseConnectionAsync();

            // 执行子类特定的数据库初始化逻辑
            await OnDatabaseInitializeInternalAsync();

            lock (_syncLock)
            {
                IsDatabaseInitialized = true;
            }

            OnInfo($"模块 {ModuleName} 数据库初始化完成");
            return true;
        }
        catch (Exception ex)
        {
            OnError($"模块 {ModuleName} 数据库初始化失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 初始化数据库连接
    /// </summary>
    /// <returns></returns>
    protected virtual async Task InitializeDatabaseConnectionAsync()
    {
        try
        {
            // 获取数据库配置
            var dbConfig = await ConfigManager.Instance.LoadDbConfigAsync();
            var sugarConfig = dbConfig.ToSugarConfig();

            DatabaseConnection = new Sugar(sugarConfig);
            OnInfo($"模块 {ModuleName} 数据库连接初始化完成");
        }
        catch (Exception ex)
        {
            OnError($"模块 {ModuleName} 数据库连接初始化失败", ex);
            throw;
        }
    }

    /// <summary>
    /// 子类可重写的数据库初始化方法
    /// </summary>
    /// <returns></returns>
    protected virtual async Task OnDatabaseInitializeInternalAsync()
    {
        // 默认实现为空，子类可以重写
        await Task.CompletedTask;
    }

    /// <summary>
    /// 获取数据库连接
    /// </summary>
    /// <returns>数据库连接实例</returns>
    protected Sugar? GetDatabaseConnection()
    {
        return DatabaseConnection;
    }

    /// <summary>
    /// 检查数据库连接是否可用
    /// </summary>
    /// <returns>连接是否可用</returns>
    protected bool IsDatabaseConnectionAvailable()
    {
        return DatabaseConnection != null && IsDatabaseInitialized;
    }

    /// <summary>
    /// 执行数据库操作的安全包装方法
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="operation">数据库操作</param>
    /// <param name="defaultValue">操作失败时的默认返回值</param>
    /// <returns>操作结果</returns>
    protected async Task<T> ExecuteDatabaseOperationAsync<T>(Func<Sugar, Task<T>> operation, T defaultValue = default(T))
    {
        try
        {
            if (!IsDatabaseConnectionAvailable())
            {
                OnError($"模块 {ModuleName} 数据库连接不可用", null);
                return defaultValue;
            }

            return await operation(DatabaseConnection!);
        }
        catch (Exception ex)
        {
            OnError($"模块 {ModuleName} 数据库操作失败", ex);
            return defaultValue;
        }
    }

    /// <summary>
    /// 执行数据库操作的安全包装方法（无返回值）
    /// </summary>
    /// <param name="operation">数据库操作</param>
    /// <returns>操作是否成功</returns>
    protected async Task<bool> ExecuteDatabaseOperationAsync(Func<Sugar, Task> operation)
    {
        try
        {
            if (!IsDatabaseConnectionAvailable())
            {
                OnError($"模块 {ModuleName} 数据库连接不可用", null);
                return false;
            }

            await operation(DatabaseConnection!);
            return true;
        }
        catch (Exception ex)
        {
            OnError($"模块 {ModuleName} 数据库操作失败", ex);
            return false;
        }
    }

    /// <summary>
    /// 清理数据库资源
    /// </summary>
    protected virtual async Task CleanupDatabaseResourcesAsync()
    {
        try
        {
            if (DatabaseConnection != null)
            {
                // 执行子类特定的清理逻辑
                await OnDatabaseCleanupAsync();

                // 清理连接
                DatabaseConnection = null;

                lock (_syncLock)
                {
                    IsDatabaseInitialized = false;
                }

                OnInfo($"模块 {ModuleName} 数据库资源清理完成");
            }
        }
        catch (Exception ex)
        {
            OnError($"模块 {ModuleName} 数据库资源清理失败", ex);
        }
    }

    /// <summary>
    /// 子类可重写的数据库清理方法
    /// </summary>
    /// <returns></returns>
    protected virtual async Task OnDatabaseCleanupAsync()
    {
        // 默认实现为空，子类可以重写
        await Task.CompletedTask;
    }

    #endregion
}