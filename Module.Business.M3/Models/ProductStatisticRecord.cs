using Core.Models;
using SqlSugar;

namespace Module.Business.SG141.Models;

/// <summary>
/// 生产统计记录
/// </summary>
[SugarTable(TableDescription = "生产统计记录")]
public class ProductStatisticRecord: RecordBase
{
    public ProductStatisticRecord()
    {
        CreateTime = DateTime.Now;
    }
    
    [SugarColumn(ColumnDescription = "统计时间", IsNullable = true)]
    public DateTime StatisticTime { get; set; } = DateTime.Now;

    [SugarColumn(ColumnDescription = "合格品数量")]
    public int OkCount { get; set; } = 0;

    [SugarColumn(ColumnDescription = "缺陷品数量")]
    public int NgCount { get; set; } = 0;

}