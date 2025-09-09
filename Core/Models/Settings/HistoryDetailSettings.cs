using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Core.Models.Settings;

/// <summary>
/// 历史查询详情设置
/// </summary>
public class HistoryDetailSettings : ConfigBase
{
    /// <summary>
    /// 历史详情配置列表
    /// </summary>
    [JsonProperty("configs")]
    public List<HistoryDetailConfig> Configs { get; set; } = new();
}

/// <summary>
/// 历史详情配置
/// </summary>
public class HistoryDetailConfig
{
    /// <summary>
    /// 标题, 用来生成对应的Tab或按钮
    /// </summary>
    [Required(ErrorMessage = "标题不能为空")]
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 对应的模型类名, 用来自动匹配表及服务
    /// </summary>
    [Required(ErrorMessage = "模型类名不能为空")]
    [JsonProperty("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// 排序字段, 每个历史数据均继承了RecordBase,有ID
    /// </summary>
    [JsonProperty("orderBy")]
    public string OrderBy { get; set; } = "Id";

    /// <summary>
    /// 排序方式
    /// </summary>
    [JsonProperty("orderByType")]
    public string OrderByType { get; set; } = "asc";

    /// <summary>
    /// 是否展示图表
    /// </summary>
    [JsonProperty("showPlot")]
    public bool ShowPlot { get; set; }

    /// <summary>
    /// 是否显示图片, 有的历史数据是检测类的数据，包含了图片地址字段
    /// </summary>
    [JsonProperty("showImage")]
    public bool ShowImage { get; set; }

    /// <summary>
    /// 需要展示的列，与HiddenColumns互斥
    /// 如果指定了Columns，则HiddenColumns无效
    /// 如 "创建时间" 或者 "create_time"
    /// </summary>
    [JsonProperty("columns")]
    public List<string> Columns { get; set; } = new();

    /// <summary>
    /// 需要隐藏的列(列注释或列名)
    /// 如 "创建时间" 或者 "create_time"
    /// </summary>
    [JsonProperty("hiddenColumns")]
    public List<string> HiddenColumns { get; set; } = new();

    /// <summary>
    /// 图表定义列表
    /// </summary>
    [JsonProperty("plots")]
    public ObservableCollection<PlotDefinition> Plots { get; set; } = new();

    /// <summary>
    /// 检测类是否展示图片
    /// 正常情况下一个零件就一张图片
    /// </summary>
    [JsonProperty("image")]
    public ImageDefinition Image { get; set; } = new();
}

/// <summary>
/// 列定义
/// </summary>
public class ColumnDefinition
{
    /// <summary>
    /// 列名
    /// </summary>
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 列标题
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 是否可见
    /// </summary>
    [JsonProperty("visible")]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// 排序
    /// </summary>
    [JsonProperty("sort")]
    public int Sort { get; set; }
}

/// <summary>
/// 图表定义，当ShowPlot为true时生效
/// </summary>
public class PlotDefinition
{
    /// <summary>
    /// 图表标题
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// X 轴标签
    /// </summary>
    [JsonProperty("xLabel")]
    public string XLabel { get; set; } = string.Empty;

    /// <summary>
    /// Y 轴标签
    /// </summary>
    [JsonProperty("yLabel")]
    public string YLabel { get; set; } = string.Empty;

    /// <summary>
    /// 定义哪些列应该显示在图表中(对应列的Key)
    /// </summary>
    [JsonProperty("columns")]
    public List<string> Columns { get; set; } = new();

    /// <summary>
    /// 数据分组列名称，比如根据机器人标识进行数据分组，避免多个机器人数据混淆
    /// 默认不分组
    /// </summary>
    [JsonProperty("groupColumnName")]
    public string GroupColumnName { get; set; } = string.Empty;
}

/// <summary>
/// 用于检测类展示图片类型
/// </summary>
public class ImageDefinition
{
    /// <summary>
    /// 图片的文件夹路径
    /// </summary>
    [JsonProperty("folderPath")]
    public string? FolderPath { get; set; }

    /// <summary>
    /// 图片的文件名
    /// </summary>
    [JsonProperty("fileName")]
    public string? FileName { get; set; }

    /// <summary>
    /// 使用哪个列的值作为图片的文件名
    /// 正常情况下，图片文件会使用零件编码作为文件名，也就是SerialNo字段
    /// 如果指定了FileName 的情况，会优先使用FileName字段的值作为文件名
    /// </summary>
    [JsonProperty("column")]
    public string? Column { get; set; }

    /// <summary>
    /// 图片的扩展名，默认为jpg
    /// </summary>
    [JsonProperty("extension")]
    public string Extension { get; set; } = "jpg";
}
