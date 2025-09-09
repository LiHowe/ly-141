using Core.Models;

namespace Core.Alarm;
public interface IAlarmService
{
	Task<AlarmRecord> TriggerAlarmAsync(AlarmRecord alarmEvent);
	Task ResolveAlarmAsync(long alarmId, string resolvedBy, string remarks);
	Task<List<AlarmRecord>> QueryRecentAlarmsAsync(int topN = 50);
	Task<PagedList<AlarmRecord>> QueryAlarmsAsync(AlarmQuery query);
}