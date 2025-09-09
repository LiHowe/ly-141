using System.Collections.ObjectModel;
using Connection.S7;
using Newtonsoft.Json;

namespace Core.Models.Settings;

/// <summary>
/// 通信设置配置
/// 包含网络通信、设备连接、协议配置等
/// </summary>
public class PlcSettings : ConfigBase
{
    [JsonProperty("configs")]
    public ObservableCollection<S7PlcConfig> Configs = new();
    
    /// <summary>
    /// 获取指定Key的PLC配置
    /// </summary>
    /// <param name="key"> PLC标识</param>
    /// <returns> PLC配置</returns>
    public S7PlcConfig? Get(string key)
    {
        return Configs.FirstOrDefault(x => x.Key == key);
    }

    /// <summary>
    /// 获取指定配置的PLC
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public S7Plc? GetPlc(string key)
    {
        var config = Get(key);
        if (config == null) return null;
        return config.GetPlc();
    }
}
