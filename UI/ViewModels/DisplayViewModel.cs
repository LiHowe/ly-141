using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.ViewModels;

/// <summary>
///     显示项ViewModel
/// </summary>
public partial class DisplayViewModel : ObservableObject
{
    /// <summary>
    ///     键值
    /// </summary>
    [ObservableProperty] private string _key = string.Empty;

    /// <summary>
    ///     主标签
    /// </summary>
    [ObservableProperty] private string _label = string.Empty;

    /// <summary>
    ///     副标签
    /// </summary>
    [ObservableProperty] private string _subLabel = string.Empty;

    /// <summary>
    ///     单位
    /// </summary>
    [ObservableProperty] private string _unit = string.Empty;

    /// <summary>
    ///     值
    /// </summary>
    [ObservableProperty] private string _value = "-";
}