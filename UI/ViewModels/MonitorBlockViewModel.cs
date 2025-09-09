using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using UI.Controls;

namespace UI.ViewModels;

/// <summary>
/// 监测控件视图模型
/// </summary>
public partial class MonitorBlockViewModel : ObservableObject
{
    #region 可观察属性

    /// <summary>
    /// 标题
    /// </summary>
    [ObservableProperty] private string _title = "点焊机器人";

    /// <summary>
    /// VIN码
    /// </summary>
    [ObservableProperty] private string _vin = "-";

    /// <summary>
    /// 运行状态
    /// </summary>
    [ObservableProperty] private MonitorBlockStatus _status = MonitorBlockStatus.Offline;

    /// <summary>
    /// 状态指示器画刷
    /// </summary>
    [ObservableProperty] private Brush _statusBrush = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));

    #endregion

    #region 公共属性

    /// <summary>
    /// 显示项集合
    /// </summary>
    [ObservableProperty] private ObservableCollection<DisplayViewModel> _displayItems;

    /// <summary>
    /// 图表项集合
    /// </summary>
    [ObservableProperty] private ObservableCollection<PlotViewModel> _plotItems;

    /// <summary>
    /// 图表网格列数
    /// </summary>
    public int ChartColumns => CalculateChartColumns();

    /// <summary>
    /// 图表网格行数
    /// </summary>
    public int ChartRows => CalculateChartRows();

    partial void OnStatusChanged(MonitorBlockStatus oldValue, MonitorBlockStatus newValue)
    {
        UpdateStatusBrush();
    }

    #endregion

    public MonitorBlockViewModel()
    {
        DisplayItems = new ObservableCollection<DisplayViewModel>();
        PlotItems = new ObservableCollection<PlotViewModel>();

        // 监听集合变更以更新布局
        // PlotItems.CollectionChanged += (s, e) =>
        // {
        //     OnPropertyChanged(nameof(ChartColumns));
        //     OnPropertyChanged(nameof(ChartRows));
        // };

        InitializeDefaultData();
    }
    
    #region 私有方法

    /// <summary>
    /// 初始化默认数据
    /// </summary>
    private void InitializeDefaultData()
    {
        UpdateStatusBrush();
    }

    /// <summary>
    /// 更新状态画刷
    /// </summary>
    private void UpdateStatusBrush()
    {
        Color baseColor = Status switch
        {
            MonitorBlockStatus.Standby => Color.FromRgb(0x2E, 0x7D, 0x32),
            MonitorBlockStatus.Running => Color.FromRgb(0x4C, 0xAF, 0x50),
            MonitorBlockStatus.Error => Color.FromRgb(0xF4, 0x43, 0x36),
            MonitorBlockStatus.Offline => Color.FromRgb(0x9E, 0x9E, 0x9E),
            _ => Color.FromRgb(0x9E, 0x9E, 0x9E),
        };

        StatusBrush = new RadialGradientBrush()
        {
            GradientOrigin = new Point(0.3, 0.3),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.6,
            RadiusY = 0.6,
            GradientStops = new GradientStopCollection()
            {
                new GradientStop(baseColor, 0.0),
                new GradientStop(Color.FromArgb(255, 
                    (byte)Math.Min(baseColor.R + 70, 255),
                    (byte)Math.Min(baseColor.G + 70, 255),
                    (byte)Math.Min(baseColor.B + 70, 255)), 0.7),
                new GradientStop(Color.FromArgb(180, baseColor.R, baseColor.G, baseColor.B), 1.0),
            }
        };
    }

    /// <summary>
    /// 计算图表网格列数
    /// </summary>
    /// <returns>列数</returns>
    private int CalculateChartColumns()
    {
        var chartCount = PlotItems.Count;
        if (chartCount == 0) return 1;

        return chartCount switch
        {
            1 => 1,
            <= 2 => 2,
            <= 4 => 2,
            _ => (int)Math.Ceiling(Math.Sqrt(chartCount))
        };
    }

    /// <summary>
    /// 计算图表网格行数
    /// </summary>
    /// <returns>行数</returns>
    private int CalculateChartRows()
    {
        var chartCount = PlotItems.Count;
        if (chartCount == 0) return 1;

        return chartCount switch
        {
            1 => 1,
            <= 2 => 1,
            <= 4 => 2,
            _ => (int)Math.Ceiling((double)chartCount / CalculateChartColumns())
        };
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 添加显示项
    /// </summary>
    /// <param name="key">键值</param>
    /// <param name="controlConfig">显示配置</param>
    public void AddDisplay(string key, DisplayControlConfig controlConfig)
    {
        if (string.IsNullOrEmpty(key) || controlConfig == null)
            return;

        // 如果已存在，先移除
        var existingItem = Enumerable.FirstOrDefault<DisplayViewModel>(DisplayItems, x => x.Key == key);
        if (existingItem != null)
        {
            DisplayItems.Remove(existingItem);
        }

        // 创建新的显示项
        var displayItem = new DisplayViewModel
        {
            Key = key,
            Label = controlConfig.Label,
            SubLabel = controlConfig.SubLabel,
            Unit = controlConfig.Unit,
            Value = controlConfig.Value
        };

        DisplayItems.Add(displayItem);
    }

    /// <summary>
    /// 设置显示项的值
    /// </summary>
    /// <param name="key">键值</param>
    /// <param name="value">新值</param>
    public void SetDisplay(string key, string value)
    {
        var item = Enumerable.FirstOrDefault<DisplayViewModel>(DisplayItems, x => x.Key == key);
        if (item != null)
        {
            item.Value = value;
        }
    }

    public void ResetAllDisplay()
    {
        foreach (var item in DisplayItems)
        { 
            item.Value = "-";
        }
    }

    /// <summary>
    /// 移除显示项
    /// </summary>
    /// <param name="key">键值</param>
    public void RemoveDisplay(string key)
    {
        var item = Enumerable.FirstOrDefault<DisplayViewModel>(DisplayItems, x => x.Key == key);
        if (item != null)
        {
            DisplayItems.Remove(item);
        }
    }

    /// <summary>
    /// 添加图表, 根据图表名称为Key
    /// </summary>
    /// <param name="title">图表名称</param>
    /// <param name="config">图表配置</param>
    public void AddPlot(PlotViewModel config)
    {
        if (config == null) return;

        // 如果已存在，先移除
        var existingItem = Enumerable.FirstOrDefault<PlotViewModel>(PlotItems, x => x.Key == config.Key);
        if (existingItem != null)
        {
            PlotItems.Remove(existingItem);
        }

        PlotItems.Add(config);
    }

    public void AddPlotSeries(string key, DataSeries series)
    {
        var item = Enumerable.FirstOrDefault<PlotViewModel>(PlotItems, x => x.Key == key);
        if (item != null)
        {
            item.DataSeries.Add(series);
        }
    }
    
    public void SetPlotSeries(string key, List<DataSeries> series)
    {
        var item = Enumerable.FirstOrDefault<PlotViewModel>(PlotItems, x => x.Key == key);
        if (item != null)
        {
            item.DataSeries = new(series);
        }
    }
    
    /// <summary>
    ///  清除图表数据
    /// </summary>
    public void ResetAllPlots()
    {
        foreach (var item in PlotItems)
        {
            foreach (var series in item.DataSeries)
            {
                series.DataY?.Clear();
                series.DataX?.Clear();
            }
        }
       
    }

    /// <summary>
    /// 移除图表
    /// </summary>
    /// <param name="key">键值</param>
    public void RemovePlot(string key)
    {
        var item = Enumerable.FirstOrDefault<PlotViewModel>(PlotItems, x => x.Key == key);
        if (item != null)
        {
            PlotItems.Remove(item);
        }
    }
    
    #endregion
}

/// <summary>
/// 点焊机器人运行状态枚举
/// </summary>
public enum MonitorBlockStatus
{
    /// <summary>
    /// 待机状态 - 深暗色绿色
    /// </summary>
    Standby,

    /// <summary>
    /// 运行状态 - 亮绿色
    /// </summary>
    Running,

    /// <summary>
    /// 错误状态 - 红色
    /// </summary>
    Error,

    /// <summary>
    /// 离线状态 - 灰色
    /// </summary>
    Offline
}