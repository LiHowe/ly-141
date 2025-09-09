using System.Reflection;

using Core.Models;
using Core.Models.Records;
using Core.Services;
using Core.Utils;
using Data.SqlSugar;
using Logger;

namespace MainApp.Initializers;

public class DatabaseInitializer : IInitializer
{
    public string Name => "数据库";

    /// <summary>
    ///     初始化数据库表结构
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static async Task Initialize()
    {
        try
        {
            if (await ReadInitializationFlag())
            {
                Log.Info("数据库表结构已初始化，跳过初始化");
                return;
            }

            // 加载本地数据库配置
            var databaseSettings = await ConfigManager.Instance.LoadDbConfigAsync();
            var sugarConfig = databaseSettings.ToSugarConfig();
            sugarConfig.EnableLogging = false;
            Sugar s = new(sugarConfig);
            s.CreateDatabase();
            AutoRegisterTables(s);
            await WriteInitializationFlag(true); // 设置初始化标志为true
            Log.Info("初始化数据库表结构完成");
        }
        catch (Exception ex)
        {
            Log.Error("初始化数据库表结构失败", ex);
        }
    }

    /// <summary>
    ///     自动注册表， 类需要继承自 RecordBase
    /// </summary>
    /// <param name="s">Sugar 实例</param>
    private static void AutoRegisterTables(object s)
    {
        var assembly = Assembly.GetAssembly(typeof(RecordBase)); // 获取包含RecordBase的程序集

        var recordTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(RecordBase).IsAssignableFrom(t))
            .ToList();
        recordTypes.Add(typeof(ProductRecord));
        recordTypes.Remove(typeof(RecordBase));
        recordTypes.Remove(typeof(BizRecordBase));

        // 获取 s 的 CreateTable<T> 泛型方法定义
        var methodInfo = s.GetType().GetMethod("CreateTable").MakeGenericMethod(typeof(object));
        // 这里先获取方法定义（占位），后面具体传入类型

        foreach (var type in recordTypes)
        {
            var genericMethod = s.GetType().GetMethod("CreateTable").MakeGenericMethod(type);
            genericMethod.Invoke(s, null);
        }
    }

    private static async Task<bool> ReadInitializationFlag()
    {
        var sysConfig = await ConfigManager.Instance.LoadSystemConfigAsync();
        return sysConfig.IsTablesInitialized;
    }

    private static async Task WriteInitializationFlag(bool flag)
    {
        var sysConfig = await ConfigManager.Instance.LoadSystemConfigAsync();
        sysConfig.IsTablesInitialized = flag;
        await ConfigManager.Instance.SaveSystemConfigAsync(sysConfig);
    }
}