using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ScottPlot;
using ScottPlot.Palettes;
using UI.ViewModels;

namespace UI.Controls;

/// <summary>
///     ScottPlot 的 WPF 包装控件，支持绘制多条动态数据线（Signal 或 Scatter 图）
/// </summary>
public partial class PlotControl : UserControl, IDisposable
{
    /// <summary>
    ///     ViewModel 的依赖属性，用于绑定 PlotViewModel
    /// </summary>
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(PlotViewModel), typeof(PlotControl),
            new PropertyMetadata(null, OnViewModelChanged));

    /// <summary>
    ///     标记控件是否已初始化
    /// </summary>
    private bool _isInitialized;

    /// <summary>
    ///     构造函数，初始化控件并设置图表样式
    /// </summary>
    public PlotControl()
    {
        InitializeComponent();
        InitPlotStyle();
        Loaded += PlotControl_Loaded; // 订阅 Loaded 事件以初始化绑定
    }

    /// <summary>
    ///     获取或设置绑定的 PlotViewModel
    /// </summary>
    public PlotViewModel ViewModel
    {
        get => (PlotViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    /// <summary>
    ///     释放资源，取消事件订阅
    /// </summary>
    public void Dispose()
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            if (ViewModel.DataSeries != null)
            {
                ViewModel.DataSeries.CollectionChanged -= DataSeries_CollectionChanged;
                foreach (var series in ViewModel.DataSeries)
                {
                    if (series.DataX != null)
                        series.DataX.CollectionChanged -= Data_CollectionChanged;
                    if (series.DataY != null)
                        series.DataY.CollectionChanged -= Data_CollectionChanged;
                }
            }
        }
    }

    /// <summary>
    ///     控件加载完成后初始化 ViewModel 绑定
    /// </summary>
    private void PlotControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (!_isInitialized && ViewModel != null)
        {
            InitializeViewModelBindings();
            _isInitialized = true;
        }
    }

    /// <summary>
    ///     ViewModel 更改时的回调，处理事件订阅
    /// </summary>
    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (PlotControl)d;
        // 取消旧 ViewModel 的事件订阅
        if (e.OldValue is PlotViewModel oldViewModel)
        {
            oldViewModel.PropertyChanged -= control.ViewModel_PropertyChanged;
            if (oldViewModel.DataSeries != null)
            {
                oldViewModel.DataSeries.CollectionChanged -= control.DataSeries_CollectionChanged;
                foreach (var series in oldViewModel.DataSeries)
                {
                    if (series.DataX != null)
                        series.DataX.CollectionChanged -= control.Data_CollectionChanged;
                    if (series.DataY != null)
                        series.DataY.CollectionChanged -= control.Data_CollectionChanged;
                }
            }
        }

        // 为新 ViewModel 初始化绑定
        if (e.NewValue is PlotViewModel newViewModel && control._isInitialized) control.InitializeViewModelBindings();
    }

    /// <summary>
    ///     初始化 ViewModel 的事件订阅并更新图表
    /// </summary>
    private void InitializeViewModelBindings()
    {
        if (ViewModel == null) return;

        // 订阅 ViewModel 的属性变化事件
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        // 订阅数据系列集合的变化事件
        if (ViewModel.DataSeries != null)
        {
            ViewModel.DataSeries.CollectionChanged += DataSeries_CollectionChanged;
            // 为每个数据系列的 DataX 和 DataY 订阅变化事件
            foreach (var series in ViewModel.DataSeries)
            {
                if (series.DataX != null)
                    series.DataX.CollectionChanged += Data_CollectionChanged;
                if (series.DataY != null)
                    series.DataY.CollectionChanged += Data_CollectionChanged;
            }
        }

        UpdatePlot(); // 初始化图表
    }

    /// <summary>
    ///     初始化图表样式，设置颜色、字体和图例样式
    /// </summary>
    private void InitPlotStyle()
    {
        var plot = PlotInstance.Plot;
        plot.Add.Palette = new Penumbra(); // 设置调色板
        plot.Axes.Color(Color.FromHex("#d7d7d7")); // 设置坐标轴颜色
        plot.Grid.MajorLineColor = Color.FromHex("#404040"); // 设置网格线颜色
        plot.FigureBackground.Color = Color.FromHex("#0D1128"); // 设置图表背景色
        plot.DataBackground.Color = Color.FromHex("#0D1128"); // 设置数据区域背景色
        const int fontSize = 14;
        plot.Axes.Bottom.Label.FontSize = fontSize; // 设置 X 轴标签字体大小
        plot.Axes.Left.Label.FontSize = fontSize; // 设置 Y 轴标签字体大小
        plot.Font.Automatic(); // 自动调整字体
        var legend = plot.Legend;
        legend.FontColor = Color.FromHex("#ffffff"); // 设置图例字体颜色
        legend.BackgroundColor = Color.FromHex("#04081C"); // 设置图例背景色
        legend.OutlineStyle.Color = Color.FromHex("#d7d7d7"); // 设置图例边框颜色
    }

    /// <summary>
    ///     处理 ViewModel 属性变化，触发图表刷新
    /// </summary>
    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        Dispatcher.InvokeAsync(UpdatePlot); // 在 UI 线程更新图表
    }

    /// <summary>
    ///     处理数据系列集合变化，更新事件订阅并刷新图表
    /// </summary>
    private void DataSeries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // 处理新增的数据系列
        if (e.NewItems != null)
            foreach (DataSeries series in e.NewItems)
            {
                if (series.DataX != null)
                    series.DataX.CollectionChanged += Data_CollectionChanged;
                if (series.DataY != null)
                    series.DataY.CollectionChanged += Data_CollectionChanged;
            }

        // 处理移除的数据系列
        if (e.OldItems != null)
            foreach (DataSeries series in e.OldItems)
            {
                if (series.DataX != null)
                    series.DataX.CollectionChanged -= Data_CollectionChanged;
                if (series.DataY != null)
                    series.DataY.CollectionChanged -= Data_CollectionChanged;
            }

        Dispatcher.InvokeAsync(UpdatePlot); // 在 UI 线程更新图表
    }

    /// <summary>
    ///     处理单个数据系列的 DataX 或 DataY 变化，触发图表刷新
    /// </summary>
    private void Data_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.InvokeAsync(UpdatePlot); // 在 UI 线程更新图表
    }

    /// <summary>
    ///     更新图表，重新绘制所有数据系列和配置
    /// </summary>
    private void UpdatePlot()
    {
        if (ViewModel == null) return;

        var plot = PlotInstance.Plot;
        plot.Clear(); // 清空现有图表内容

        // 设置标题和标签
        plot.Title(ViewModel.Title);
        plot.XLabel(ViewModel.XLabel);
        plot.YLabel(ViewModel.YLabel);

        // 设置图例显示
        plot.Legend.IsVisible = ViewModel.ShowLegend;

        // 绘制数据系列
        if (ViewModel.DataSeries != null)
            foreach (var series in ViewModel.DataSeries)
            {
                // 验证数据有效性
                if (series.DataY == null || series.DataY.Count == 0)
                    continue;
                if (series.DataY.Any(d => double.IsNaN(d) || double.IsInfinity(d)))
                    continue;
                if (series.DataX != null && series.DataX.Any(d => double.IsNaN(d) || double.IsInfinity(d)))
                    continue;

                var markerSize = (float)(10 * ViewModel.ScaleFactor); // 应用缩放比例到标记大小

                if (series.DataX == null || series.DataX.Count == 0)
                {
                    // 只有 DataY，绘制 Signal 图
                    Core.Utils.UIThreadHelper.InvokeAsync(() =>
                    {
                        var ys = series.DataY.ToArray();
                        var signal = plot.Add.Signal(ys);
                        signal.LegendText = series.Legend; // 设置图例名称
                        signal.MarkerSize = markerSize; // 设置数据点标记大小
                    });
                }
                else if (series.DataX.Count == series.DataY.Count)
                {
                    // 有 DataX 和 DataY，绘制 Scatter 图
                    var scatter = plot.Add.Scatter(series.DataX.ToArray(), series.DataY.ToArray());
                    scatter.LegendText = series.Legend; // 设置图例名称
                    scatter.MarkerSize = markerSize; // 设置数据点标记大小
                }
            }

        // 设置上下限线
        if (ViewModel.MaxValue != double.MaxValue)
        {
            var upperLine =
                plot.Add.HorizontalLine(ViewModel.MaxValue, 2f, ViewModel.MaxValueColor, LinePattern.Dashed);
            upperLine.Text = ViewModel.ShowMaxLabel ? $"上限 {ViewModel.MaxValue}" : ""; // 根据 ShowMaxLabel 设置标签
        }

        if (ViewModel.MinValue != double.MinValue)
        {
            var lowerLine =
                plot.Add.HorizontalLine(ViewModel.MinValue, 2f, ViewModel.MinValueColor, LinePattern.Dashed);
            lowerLine.Text = ViewModel.ShowMinLabel ? $"下限 {ViewModel.MinValue}" : ""; // 根据 ShowMinLabel 设置标签
        }

        // 根据 AutoScaleMode 设置坐标轴
        if (ViewModel.AutoScaleMode == AutoScaleMode.AutoScale)
        {
            plot.Axes.AutoScale(); // 自动缩放
        }
        else if (ViewModel.AutoScaleMode == AutoScaleMode.LatestPoints && ViewModel.DataSeries != null)
        {
            // 显示最新的 N 个点
            var pointCount = ViewModel.LatestPointCount;
            var maxX = double.MinValue;
            foreach (var series in ViewModel.DataSeries)
                if (series.DataX != null && series.DataX.Count > 0)
                    maxX = Math.Max(maxX, series.DataX.Max());
                else if (series.DataY != null && series.DataY.Count > 0) maxX = Math.Max(maxX, series.DataY.Count - 1);

            if (maxX != double.MinValue)
            {
                var minX = Math.Max(0, maxX - pointCount);
                plot.Axes.SetLimitsX(minX, maxX); // 设置 X 轴范围
                plot.Axes.AutoScaleY(); // Y 轴自动缩放
            }
            else
            {
                plot.Axes.AutoScale(); // 回退到自动缩放
            }
        }

        plot.Font.Automatic(); // 自动调整字体
        PlotInstance.Refresh(); // 刷新图表
    }
}