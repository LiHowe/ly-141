using System.Windows.Controls;
using UI.ViewModels;

namespace UI.Controls;

/// <summary>
///     DisplayControl.xaml 的交互逻辑
///     用于显示数值信息的通用控件，支持主标签、副标签、数值和单位
/// </summary>
public partial class DisplayControl : UserControl
{
    private readonly DisplayViewModel _viewModel;

    public DisplayControl()
    {
        InitializeComponent();
    }

    public DisplayControl(DisplayViewModel vm) : this()
    {
        _viewModel = vm ?? new DisplayViewModel();
        DataContext = vm;
    }

    /// <summary>
    ///     释放资源，清理 DataContext
    /// </summary>
    public void Dispose()
    {
        DataContext = null; // 清理 DataContext
    }
}

/// <summary>
/// 显示控件配置类
/// </summary>
public class DisplayControlConfig
{

    /// <summary>
    /// 用来区分与快速查找配置
    /// </summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>
    ///     主标签（中文）
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    ///     副标签（英文）
    /// </summary>
    public string SubLabel { get; set; } = string.Empty;

    /// <summary>
    ///     单位
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    ///     初始值
    /// </summary>
    public string Value { get; set; } = "-";
}