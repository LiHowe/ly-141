using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ScottPlot;

namespace UI.ViewModels;

public partial class PlotViewModel : ObservableObject
{
    /// <summary>
    ///     缩放模式（自动缩放或显示最新 N 个点）
    /// </summary>
    [ObservableProperty] private AutoScaleMode _autoScaleMode = AutoScaleMode.AutoScale;

    /// <summary>
    ///     数据系列集合，每条系列包含图例名称、X 轴和 Y 轴数据
    /// </summary>
    [ObservableProperty] private ObservableCollection<DataSeries> _dataSeries = new();

    /// <summary>
    ///     图表唯一标识
    /// </summary>
    [ObservableProperty] private string _key = string.Empty;

    /// <summary>
    ///     显示的最新点数（仅在 LatestPoints 模式下有效）
    /// </summary>
    [ObservableProperty] private int _latestPointCount = 100;

    /// <summary>
    ///     Y 轴上限值，图表中会绘制一条水平线
    /// </summary>
    [ObservableProperty] private double _maxValue = double.MaxValue;

    /// <summary>
    ///     上限线的颜色
    /// </summary>
    [ObservableProperty] private Color _maxValueColor = Color.FromHex("#f44336"); // 默认红色

    /// <summary>
    ///     Y 轴下限值，图表中会绘制一条水平线
    /// </summary>
    [ObservableProperty] private double _minValue = double.MinValue;

    /// <summary>
    ///     下限线的颜色
    /// </summary>
    [ObservableProperty] private Color _minValueColor = Color.FromHex("#2196f3"); // 默认蓝色

    /// <summary>
    ///     图表缩放比例（默认 1）
    /// </summary>
    [ObservableProperty] private double _scaleFactor = 1.0;

    /// <summary>
    ///     是否显示图例
    /// </summary>
    [ObservableProperty] private bool _showLegend = true;

    /// <summary>
    ///     是否显示上限标签
    /// </summary>
    [ObservableProperty] private bool _showMaxLabel;

    /// <summary>
    ///     是否显示下限标签
    /// </summary>
    [ObservableProperty] private bool _showMinLabel;

    /// <summary>
    ///     图表标题
    /// </summary>
    [ObservableProperty] private string _title = string.Empty;

    /// <summary>
    ///     X 轴标签
    /// </summary>
    [ObservableProperty] private string _xLabel = string.Empty;

    /// <summary>
    ///     Y 轴标签
    /// </summary>
    [ObservableProperty] private string _yLabel = string.Empty;
}

/// <summary>
///     缩放模式枚举
/// </summary>
public enum AutoScaleMode
{
    /// <summary>
    ///     自动缩放，显示所有数据
    /// </summary>
    AutoScale,

    /// <summary>
    ///     显示最新的 N 个点
    /// </summary>
    LatestPoints
}

/// <summary>
///     数据系列类，定义单条数据线的属性
/// </summary>
public partial class DataSeries : ObservableObject
{
    /// <summary>
    ///     X 轴数据集合（可为空，用于 Signal 图）
    /// </summary>
    [ObservableProperty] private ObservableCollection<double> _dataX;

    /// <summary>
    ///     Y 轴数据集合
    /// </summary>
    [ObservableProperty] private ObservableCollection<double> _dataY = new();

    /// <summary>
    ///     图例名称（如 "电流1"）
    /// </summary>
    [ObservableProperty] private string _legend = string.Empty;
}