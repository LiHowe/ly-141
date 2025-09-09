using System.Windows.Controls;
using HandyControl.Data;
using MainApp.ViewModels;

namespace MainApp.Views;

/// <summary>
///     HistoryView.xaml 的交互逻辑
/// </summary>
public partial class HistoryView : UserControl
{
    private readonly HistoryViewModel _viewModel;

    public HistoryView()
    {
        _viewModel ??= new HistoryViewModel();
        DataContext = _viewModel;
        InitializeComponent();
    }

    public HistoryView(HistoryViewModel vm)
    {
        _viewModel = vm;
        DataContext = _viewModel;
        InitializeComponent();
    }

    /// <summary>
    ///     分页控件页码变化事件处理
    /// </summary>
    private async void Pagination_PageChanged(object sender, FunctionEventArgs<int> e)
    {
        if (DataContext is HistoryViewModel viewModel) await viewModel.OnPageChangedAsync(e.Info);
    }
}