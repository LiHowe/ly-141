using UI.ViewModels;
using PlotViewModel = UI.ViewModels.PlotViewModel;

namespace UI.Controls;

public partial class MonitorBlockControl: BizControlBase
{
    private readonly MonitorBlockViewModel _viewModel;

    public MonitorBlockControl()
    {
        InitializeComponent();
    }

    public MonitorBlockControl(MonitorBlockViewModel vm) : this()
    {
        _viewModel = vm ?? new();
        DataContext = _viewModel;
    }

    #region 公共方法

    /// <summary>
    ///     添加显示控件
    /// </summary>
    /// <param name="key">控件键值</param>
    /// <param name="config">显示配置</param>
    public void AddDisplay(string key, DisplayControlConfig config)
    {
        _viewModel.AddDisplay(key, config);
    }

    /// <summary>
    ///     设置显示控件的值
    /// </summary>
    /// <param name="key">控件键值</param>
    /// <param name="value">新值</param>
    public void SetDisplay(string key, string value)
    {
        _viewModel.SetDisplay(key, value);
    }

    /// <summary>
    ///     移除显示控件
    /// </summary>
    /// <param name="key">控件键值</param>
    public void RemoveDisplay(string key)
    {
        _viewModel.RemoveDisplay(key);
    }

    /// <summary>
    ///     添加图表
    /// </summary>
    /// <param name="config">图表配置</param>
    public void AddPlot(PlotViewModel config)
    {
        _viewModel.AddPlot(config);
    }

    /// <summary>
    ///     移除图表
    /// </summary>
    /// <param name="key">图表键值</param>
    public void RemovePlot(string key)
    {
        _viewModel.RemovePlot(key);
    }

    public override void Start()
    {
        _viewModel.Status = MonitorBlockStatus.Running;
    }

    public override void Stop()
    {
        _viewModel.Status = MonitorBlockStatus.Standby;
    }

    public override void Reset()
    {
        _viewModel.Vin = "";
        _viewModel.Status = MonitorBlockStatus.Standby;
        _viewModel.ResetAllPlots();
        _viewModel.ResetAllDisplay();
    }

    public override void Error()
    {
        _viewModel.Status = MonitorBlockStatus.Error;
    }

    #endregion
}