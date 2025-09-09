using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using Core.Models.Settings;

namespace MainApp.ViewModels;

/// <summary>
///     历史详情标签页
/// </summary>
public partial class HistoryDetailTab : ObservableObject
{
    /// <summary>
    ///     配置信息
    /// </summary>
    [ObservableProperty] private HistoryDetailConfig _config = new();

    /// <summary>
    ///     数据总数
    /// </summary>
    [ObservableProperty] private int _dataCount;

    /// <summary>
    ///     历史详情数据
    /// </summary>
    [ObservableProperty] private DataTable? _detailData;

    /// <summary>
    ///     是否可见
    /// </summary>
    [ObservableProperty] private bool _isVisible = true;

    /// <summary>
    ///     标题
    /// </summary>
    [ObservableProperty] private string _title = string.Empty;

    [ObservableProperty] private bool _isSelected = false;
}