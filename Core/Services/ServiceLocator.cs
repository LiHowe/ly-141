namespace Core.Services
{
    /// <summary>
    /// 服务定位器
    /// 提供全局访问服务的静态方法
    /// </summary>
    public static class ServiceLocator
    {
        private static ServiceManager? _serviceManager;

        /// <summary>
        /// 设置服务管理器
        /// </summary>
        /// <param name="serviceManager">服务管理器实例</param>
        public static void SetServiceManager(ServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>服务实例，如果未找到则返回null</returns>
        public static T? GetService<T>() where T : class
        {
            return _serviceManager?.GetService<T>();
        }

        /// <summary>
        /// 检查服务是否已注册
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <returns>如果服务已注册则返回true</returns>
        public static bool IsServiceRegistered<T>() where T : class
        {
            return _serviceManager?.IsServiceRegistered<T>() ?? false;
        }

        /// <summary>
        /// 获取服务管理器
        /// </summary>
        /// <returns>服务管理器实例</returns>
        public static ServiceManager? GetServiceManager()
        {
            return _serviceManager;
        }

        /// <summary>
        /// 清理服务定位器
        /// </summary>
        public static void Clear()
        {
            _serviceManager = null;
        }
    }
}
