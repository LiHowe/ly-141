using Core.Alarm;
using Core.Alarm.Strategies;
using Core.Models.Settings;
using Core.Utils;
using Data.SqlSugar;

namespace Core.Services;

/// <summary>
/// 报警服务
/// </summary>
public class AlarmService
{

    public AlarmService()
    {
        AlarmCoordinator.Instance.AddStrategy(new TrayStrategy());
        var dbConfig = ConfigManager.Instance.LoadConfig<DatabaseSettings>(Constants.LocalDbConfigFilePath);
        var sugar = new Sugar(dbConfig.ToSugarConfig());
        AlarmCoordinator.Instance.AddStrategy(new DatabaseStrategy(sugar.GetDb()));
    }
}