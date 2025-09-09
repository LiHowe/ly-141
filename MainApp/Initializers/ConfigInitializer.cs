using Core.Utils;

namespace MainApp.Initializers;

/// <summary>
///     配置定义
/// </summary>
public sealed class ConfigInitializer : IInitializer
{
    public static string Name => "配置文件";

    public static async Task Initialize()
    {
        await ConfigManager.Instance.LoadPlcConfigAsync();
        // 注册配置
        //ConfigControlManager.RegisterConfig<CommonSysConfig, NormalConfigControl>();
        //ConfigControlManager.RegisterConfig<SqlSugarConfig, DbConfigControl>();
        //ConfigControlManager.RegisterConfig<S7PlcConfig, S7ConfigControl>();
        //ConfigControlManager.RegisterConfig<ImlightConfig, ImlightConfigControl>();
        //ConfigControlManager.RegisterConfig<ProductConfig, ProductConfigControl>();
        //// 初始化配置控制器
        //ConfigControlManager.Initialize();
    }
}