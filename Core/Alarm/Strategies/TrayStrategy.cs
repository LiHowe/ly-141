using HandyControl.Controls;
using HandyControl.Data;

namespace Core.Alarm.Strategies;

public class TrayStrategy: IAlarmStrategy
{
    private DateTime _lastNotifyTime = DateTime.MinValue;
    private string _lastMessageHash;
    private string _id;
    public string Id => _id;

    public TrayStrategy(string id = "")
    {
        _id = string.IsNullOrWhiteSpace(id) ?  new Guid().ToString() : id;
    }
    
    public async Task SendAsync(AlarmRecord record)
    {
        if (!ShouldNotify(record)) return;
        var type = record.Status;
        var title = GetTitle(type);
        var message = FormatMessage(record);

        var icon = record.Level switch
        {
            AlarmLevel.Error => NotifyIconInfoType.Error,
            AlarmLevel.Warning => NotifyIconInfoType.Warning,
            AlarmLevel.Info => NotifyIconInfoType.Info,
            _ => NotifyIconInfoType.None
        };
        
        NotifyIcon.ShowBalloonTip(
            title,
            message,
            icon,
            "Global"
        );
        
        UpdateNotificationState(record);
    }

    public bool ShouldNotify(AlarmRecord record)
    {
        // 5分钟内不重复相同报警
        var currentHash = GetMessageHash(record);
        return (DateTime.Now - _lastNotifyTime).TotalMinutes > 5
               || currentHash != _lastMessageHash;
    }

    private string FormatMessage(AlarmRecord record)
    {
        return $"[{record.Module}] [{record.Level}] {record.Message}";
    }

    private string GetTitle(AlarmStatus type)
    {
        return type switch
        {
            AlarmStatus.New => "新报警通知",
            AlarmStatus.Resolved => "报警已解除",
            _ => "系统通知"
        };
    }

    private void UpdateNotificationState(AlarmRecord record)
    {
        _lastNotifyTime = DateTime.Now;
        _lastMessageHash = GetMessageHash(record);
    }

    private static string GetMessageHash(AlarmRecord record)
    {
        return $"{record.Module}-{record.Category}-{record.Message}";
    }
}