using Core.Models;

namespace Core.Interfaces
{
    /// <summary>
    /// 菜单服务接口
    /// </summary>
    public interface IMenuService
    {
        /// <summary>
        /// 菜单配置变化事件
        /// </summary>
        event EventHandler<MenuConfigurationChangedEventArgs>? MenuConfigurationChanged;

        /// <summary>
        /// 获取所有菜单配置
        /// </summary>
        MenuConfigurationCollection GetMenuConfiguration();

        /// <summary>
        /// 获取当前用户可访问的菜单
        /// </summary>
        /// <param name="userRole">用户角色</param>
        List<MenuConfiguration> GetAccessibleMenus(string userRole);

        /// <summary>
        /// 获取根菜单项
        /// </summary>
        /// <param name="userRole">用户角色</param>
        List<MenuConfiguration> GetRootMenus(string userRole);

        /// <summary>
        /// 获取子菜单项
        /// </summary>
        /// <param name="parentId">父菜单ID</param>
        /// <param name="userRole">用户角色</param>
        List<MenuConfiguration> GetChildMenus(string parentId, string userRole);

        /// <summary>
        /// 根据ID获取菜单项
        /// </summary>
        /// <param name="menuId">菜单ID</param>
        MenuConfiguration? GetMenuById(string menuId);

        /// <summary>
        /// 添加菜单项
        /// </summary>
        /// <param name="menu">菜单配置</param>
        Task<bool> AddMenuAsync(MenuConfiguration menu);

        /// <summary>
        /// 更新菜单项
        /// </summary>
        /// <param name="menu">菜单配置</param>
        Task<bool> UpdateMenuAsync(MenuConfiguration menu);

        /// <summary>
        /// 删除菜单项
        /// </summary>
        /// <param name="menuId">菜单ID</param>
        Task<bool> RemoveMenuAsync(string menuId);

        /// <summary>
        /// 启用/禁用菜单项
        /// </summary>
        /// <param name="menuId">菜单ID</param>
        /// <param name="enabled">是否启用</param>
        Task<bool> SetMenuEnabledAsync(string menuId, bool enabled);

        /// <summary>
        /// 保存菜单配置
        /// </summary>
        Task SaveMenuConfigurationAsync();

        /// <summary>
        /// 重新加载菜单配置
        /// </summary>
        Task ReloadMenuConfigurationAsync();

        /// <summary>
        /// 检查用户是否有访问菜单的权限
        /// </summary>
        /// <param name="menuId">菜单ID</param>
        /// <param name="userRole">用户角色</param>
        bool HasMenuAccess(string menuId, string userRole);

        /// <summary>
        /// 从模块注册菜单
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        /// <param name="menus">菜单列表</param>
        Task RegisterModuleMenusAsync(string moduleId, List<MenuConfiguration> menus);

        /// <summary>
        /// 取消注册模块菜单
        /// </summary>
        /// <param name="moduleId">模块ID</param>
        Task UnregisterModuleMenusAsync(string moduleId);
    }

    /// <summary>
    /// 菜单配置变化事件参数
    /// </summary>
    public class MenuConfigurationChangedEventArgs : EventArgs
    {
        public string MenuId { get; set; } = string.Empty;
        public MenuChangeType ChangeType { get; set; }
        public MenuConfiguration? Menu { get; set; }

        public MenuConfigurationChangedEventArgs(string menuId, MenuChangeType changeType, MenuConfiguration? menu = null)
        {
            MenuId = menuId;
            ChangeType = changeType;
            Menu = menu;
        }
    }

    /// <summary>
    /// 菜单变化类型
    /// </summary>
    public enum MenuChangeType
    {
        Added,
        Updated,
        Removed,
        EnabledChanged,
        Reloaded
    }

    /// <summary>
    /// 用户角色枚举
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// 游客
        /// </summary>
        Guest,
        /// <summary>
        /// 系统用户
        /// </summary>
        User,
        /// <summary>
        /// 系统管理员
        /// </summary>
        Admin,
        /// <summary>
        /// 开发者
        /// </summary>
        SuperAdmin
    }
}
