namespace Core.Alarm;

public interface IAlarmStrategy
{
    string Id { get; }

    Task SendAsync(AlarmRecord record);
    bool ShouldNotify(AlarmRecord record);
}

