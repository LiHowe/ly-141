using System.Windows.Controls;
using Core.Models;

namespace Core.Interfaces
{
    /// <summary>
    /// 模块接口定义
    /// 所有功能模块都必须实现此接口
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// 模块唯一标识符
        /// </summary>
        string ModuleId { get; }

        /// <summary>
        /// 模块显示名称
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// 模块描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 模块版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 模块是否已启用
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// 模块是否已初始化
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 模块是否正在运行
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 模块图标路径
        /// </summary>
        string? IconPath { get; }

        /// <summary>
        /// 模块排序权重（用于菜单排序）
        /// </summary>
        int SortOrder { get; }

        /// <summary>
        /// 初始化模块
        /// </summary>
        /// <returns>初始化是否成功</returns>
        Task<bool> InitializeAsync();

        /// <summary>
        /// 根据页面名称创建页面实例
        /// </summary>
        /// <param name="pageName">页面名称</param>
        /// <returns>页面实例，如果页面不存在则返回null</returns>
        UserControl? CreatePage(string pageName);

        /// <summary>
        /// 模块启动时调用
        /// </summary>
        Task OnStartupAsync();

        /// <summary>
        /// 模块关闭时调用
        /// </summary>
        Task OnShutdownAsync();

        /// <summary>
        /// 模块启用时调用
        /// </summary>
        Task OnEnabledAsync();

        /// <summary>
        /// 模块禁用时调用
        /// </summary>
        Task OnDisabledAsync();

        /// <summary>
        /// 获取模块菜单配置
        /// </summary>
        /// <returns>模块的菜单配置列表</returns>
        List<MenuConfiguration> GetMenuConfigurations();

        /// <summary>
        /// 获取模块设置页面
        /// </summary>
        /// <returns>设置页面实例，如果没有设置页面则返回null</returns>
        UserControl? GetSettingsPage();

        /// <summary>
        /// 获取模块设置页面类型
        /// </summary>
        /// <returns>设置页面类型，如果没有设置页面则返回null</returns>
        Type? GetSettingsPageType();

        /// <summary>
        /// 是否有设置页面
        /// </summary>
        bool HasSettingsPage { get; }

        /// <summary>
        /// 获取模块需要的数据库表类型
        /// </summary>
        /// <returns>数据库表类型列表，如果模块不需要数据库表则返回空列表</returns>
        List<Type> GetDatabaseTableTypes();

        /// <summary>
        /// 模块数据库初始化时调用
        /// 在模块初始化过程中，数据库表创建完成后会调用此方法
        /// </summary>
        /// <returns>数据库初始化是否成功</returns>
        Task<bool> OnDatabaseInitializeAsync();
    }
}
