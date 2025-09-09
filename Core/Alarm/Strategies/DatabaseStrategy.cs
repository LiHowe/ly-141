using System.Diagnostics;
using SqlSugar;

namespace Core.Alarm.Strategies;

public class DatabaseStrategy: IAlarmStrategy
{
    public string Id => nameof(DatabaseStrategy);

    private AlarmRepository _alarmRepository;
    
    public async Task SendAsync(AlarmRecord record)
    {
        try
        {
            await _alarmRepository.SaveAlarmAsync(record);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public bool ShouldNotify(AlarmRecord record)
    {
        return true;
    }

    public DatabaseStrategy(ISqlSugarClient sugarClient)
    {
        _alarmRepository = new(sugarClient);
    }
}