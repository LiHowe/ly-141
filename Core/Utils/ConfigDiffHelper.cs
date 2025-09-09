using System.Reflection;
using Newtonsoft.Json;

namespace Core.Utils
{
    /// <summary>
    /// 配置差异检测工具类
    /// 用于检测配置对象之间的差异
    /// </summary>
    public static class ConfigDiffHelper
    {
        /// <summary>
        /// 比较两个配置对象，返回变更的属性名称列表
        /// </summary>
        /// <typeparam name="T">配置对象类型</typeparam>
        /// <param name="oldConfig">旧配置对象</param>
        /// <param name="newConfig">新配置对象</param>
        /// <returns>变更的属性名称列表</returns>
        public static List<string> GetChangedProperties<T>(T? oldConfig, T? newConfig) where T : class
        {
            var changedProperties = new List<string>();

            if (oldConfig == null && newConfig == null)
                return changedProperties;

            if (oldConfig == null || newConfig == null)
            {
                // 如果其中一个为null，则认为所有属性都发生了变更
                var type = (oldConfig ?? newConfig)!.GetType();
                return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                          .Where(p => p.CanRead)
                          .Select(p => p.Name)
                          .ToList();
            }

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                     .Where(p => p.CanRead);

            foreach (var property in properties)
            {
                try
                {
                    var oldValue = property.GetValue(oldConfig);
                    var newValue = property.GetValue(newConfig);

                    if (!AreEqual(oldValue, newValue))
                    {
                        changedProperties.Add(property.Name);
                    }
                }
                catch (Exception ex)
                {
                    // 记录错误但继续处理其他属性
                    System.Diagnostics.Debug.WriteLine($"比较属性 {property.Name} 时发生错误: {ex.Message}");
                }
            }

            return changedProperties;
        }

        /// <summary>
        /// 使用JSON序列化比较两个配置对象的差异
        /// </summary>
        /// <typeparam name="T">配置对象类型</typeparam>
        /// <param name="oldConfig">旧配置对象</param>
        /// <param name="newConfig">新配置对象</param>
        /// <returns>变更的属性名称列表</returns>
        public static List<string> GetChangedPropertiesUsingJson<T>(T? oldConfig, T? newConfig) where T : class
        {
            var changedProperties = new List<string>();

            if (oldConfig == null && newConfig == null)
                return changedProperties;

            try
            {
                var oldJson = oldConfig != null ? JsonConvert.SerializeObject(oldConfig) : "null";
                var newJson = newConfig != null ? JsonConvert.SerializeObject(newConfig) : "null";

                if (oldJson == newJson)
                    return changedProperties;

                // 如果JSON不同，则进行详细的属性比较
                return GetChangedProperties(oldConfig, newConfig);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"使用JSON比较配置时发生错误: {ex.Message}");
                // 如果JSON序列化失败，回退到反射比较
                return GetChangedProperties(oldConfig, newConfig);
            }
        }

        /// <summary>
        /// 比较两个值是否相等
        /// </summary>
        /// <param name="value1">值1</param>
        /// <param name="value2">值2</param>
        /// <returns>是否相等</returns>
        private static bool AreEqual(object? value1, object? value2)
        {
            if (value1 == null && value2 == null)
                return true;

            if (value1 == null || value2 == null)
                return false;

            // 对于集合类型，使用JSON序列化比较
            if (IsCollectionType(value1.GetType()) || IsCollectionType(value2.GetType()))
            {
                try
                {
                    var json1 = JsonConvert.SerializeObject(value1);
                    var json2 = JsonConvert.SerializeObject(value2);
                    return json1 == json2;
                }
                catch
                {
                    // 如果JSON序列化失败，使用默认比较
                    return value1.Equals(value2);
                }
            }

            // 对于复杂对象，使用JSON序列化比较
            if (IsComplexType(value1.GetType()))
            {
                try
                {
                    var json1 = JsonConvert.SerializeObject(value1);
                    var json2 = JsonConvert.SerializeObject(value2);
                    return json1 == json2;
                }
                catch
                {
                    // 如果JSON序列化失败，使用默认比较
                    return value1.Equals(value2);
                }
            }

            return value1.Equals(value2);
        }

        /// <summary>
        /// 检查类型是否为集合类型
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否为集合类型</returns>
        private static bool IsCollectionType(Type type)
        {
            return type.IsArray ||
                   (type.IsGenericType && (
                       type.GetGenericTypeDefinition() == typeof(List<>) ||
                       type.GetGenericTypeDefinition() == typeof(IList<>) ||
                       type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                       type.GetGenericTypeDefinition() == typeof(IEnumerable<>))) ||
                   type.GetInterfaces().Any(i => 
                       i.IsGenericType && (
                           i.GetGenericTypeDefinition() == typeof(IList<>) ||
                           i.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                           i.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
        }

        /// <summary>
        /// 检查类型是否为复杂类型（非基本类型）
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否为复杂类型</returns>
        private static bool IsComplexType(Type type)
        {
            return !type.IsPrimitive &&
                   type != typeof(string) &&
                   type != typeof(DateTime) &&
                   type != typeof(decimal) &&
                   type != typeof(Guid) &&
                   !type.IsEnum &&
                   !(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// 获取配置对象的摘要信息
        /// </summary>
        /// <typeparam name="T">配置对象类型</typeparam>
        /// <param name="config">配置对象</param>
        /// <returns>配置摘要</returns>
        public static string GetConfigSummary<T>(T? config) where T : class
        {
            if (config == null)
                return "null";

            try
            {
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                         .Where(p => p.CanRead)
                                         .Take(5) // 只取前5个属性作为摘要
                                         .Select(p => $"{p.Name}={p.GetValue(config)}")
                                         .ToList();

                return $"{typeof(T).Name}({string.Join(", ", properties)})";
            }
            catch (Exception ex)
            {
                return $"{typeof(T).Name}(Error: {ex.Message})";
            }
        }
    }
}
