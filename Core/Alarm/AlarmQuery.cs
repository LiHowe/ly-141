using Core.Interfaces;

namespace Core.Alarm;

public class AlarmQuery: IPagedQuery
{
    public int PageIndex { get; set; } = 1;
    
    private int _pageSize = 50;
    /// <summary>
    /// 表示每页的记录数量，默认值为 50，最大值为 100。
    /// </summary>
    public int PageSize 
    { 
        get => _pageSize; 
        set => _pageSize = value < 1 ? 50 : Math.Min(value, 100); 
    }
    
    public List<AlarmLevel> Levels { get; set; }
    public string Module { get; set; }
    public DateTime? StartTime { get; set; }
}