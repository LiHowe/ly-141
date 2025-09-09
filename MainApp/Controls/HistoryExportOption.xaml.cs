using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Models.Records;
using Core.Models.Settings;
using Microsoft.Win32;
using MessageBox = HandyControl.Controls.MessageBox;

namespace MainApp.Controls;

/// <summary>
///     历史数据导出选项控件
/// </summary>
public partial class HistoryExportOption : UserControl
{
    private readonly HistoryExportOptionViewModel _viewModel;

    public HistoryExportOption()
    {
        InitializeComponent();
        _viewModel = new HistoryExportOptionViewModel();
        DataContext = _viewModel;
    }

    public HistoryExportOption(ProductRecord? selectedPart, List<HistoryDetailConfig> availableConfigs)
    {
        InitializeComponent();
        _viewModel = new HistoryExportOptionViewModel(selectedPart, availableConfigs);
        DataContext = _viewModel;
    }

    /// <summary>
    ///     导出完成事件
    /// </summary>
    public event EventHandler<ExportCompletedEventArgs>? ExportCompleted;

    /// <summary>
    ///     取消事件
    /// </summary>
    public event EventHandler? Cancelled;

    private void OnExportCompleted(bool success, string message)
    {
        ExportCompleted?.Invoke(this, new ExportCompletedEventArgs(success, message));
    }

    private void OnCancelled()
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
///     历史数据导出选项视图模型
/// </summary>
public partial class HistoryExportOptionViewModel : ObservableObject
{
    private readonly ProductRecord? _selectedPart;
    private CancellationTokenSource? _cancellationTokenSource;

    public HistoryExportOptionViewModel()
    {
        InitializeExportTypes();
    }

    public HistoryExportOptionViewModel(ProductRecord? selectedPart, List<HistoryDetailConfig> availableConfigs)
    {
        _selectedPart = selectedPart;
        InitializeExportTypes(availableConfigs);
        UpdateSelectedPartInfo();
    }

    #region 属性

    /// <summary>
    ///     导出全部数据
    /// </summary>
    [ObservableProperty] private bool _exportAll = true;

    /// <summary>
    ///     导出选中数据
    /// </summary>
    [ObservableProperty] private bool _exportSelected;

    /// <summary>
    ///     是否有选中的零件
    /// </summary>
    [ObservableProperty] private bool _hasSelectedPart;

    /// <summary>
    ///     选中零件信息
    /// </summary>
    [ObservableProperty] private string _selectedPartInfo = string.Empty;

    /// <summary>
    ///     导出类型列表
    /// </summary>
    [ObservableProperty] private ObservableCollection<ExportTypeInfo> _exportTypes = new();

    /// <summary>
    ///     选择统计
    /// </summary>
    [ObservableProperty] private string _selectionSummary = string.Empty;

    /// <summary>
    ///     是否正在导出
    /// </summary>
    [ObservableProperty] private bool _isExporting;

    /// <summary>
    ///     导出进度
    /// </summary>
    [ObservableProperty] private double _exportProgress;

    /// <summary>
    ///     是否可以导出
    /// </summary>
    [ObservableProperty] private bool _canExport = true;

    #endregion

    #region 命令

    /// <summary>
    ///     全选命令
    /// </summary>
    [RelayCommand]
    private void SelectAll()
    {
        foreach (var type in ExportTypes) type.IsSelected = true;
        UpdateSelectionSummary();
    }

    /// <summary>
    ///     反选命令
    /// </summary>
    [RelayCommand]
    private void InvertSelection()
    {
        foreach (var type in ExportTypes) type.IsSelected = !type.IsSelected;
        UpdateSelectionSummary();
    }

    /// <summary>
    ///     导出命令
    /// </summary>
    [RelayCommand]
    private async Task Export()
    {
        var selectedTypes = ExportTypes.Where(t => t.IsSelected).ToList();
        if (!selectedTypes.Any())
        {
            MessageBox.Show("请至少选择一种导出类型！", "提示",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 选择保存文件夹
        var dialog = new SaveFileDialog
        {
            Title = "选择导出文件夹",
            Filter = "文件夹|*.*",
            FileName = "选择文件夹"
        };

        if (dialog.ShowDialog() != true)
            return;

        var exportPath = Path.GetDirectoryName(dialog.FileName) ??
                         Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        try
        {
            IsExporting = true;
            CanExport = false;
            ExportProgress = 0;

            _cancellationTokenSource = new CancellationTokenSource();

            await ExportDataAsync(exportPath, selectedTypes, _cancellationTokenSource.Token);

            MessageBox.Show($"导出完成！\n文件保存在：{exportPath}", "导出成功",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show("导出已取消", "提示",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"导出失败：{ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsExporting = false;
            CanExport = true;
            ExportProgress = 0;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    /// <summary>
    ///     取消命令
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        if (IsExporting)
        {
            _cancellationTokenSource?.Cancel();
        }
        else
        {
            // 触发取消事件
            if (Application.Current.MainWindow is Window mainWindow)
            {
                var exportControl = mainWindow.FindName("HistoryExportOption") as HistoryExportOption;
                // 这里需要通过事件或其他方式通知父窗口关闭
            }
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    ///     初始化导出类型
    /// </summary>
    private void InitializeExportTypes(List<HistoryDetailConfig>? configs = null)
    {
        ExportTypes.Clear();

        // 添加零件列表
        ExportTypes.Add(new ExportTypeInfo
        {
            TypeName = "ProductRecord",
            DisplayName = "零件列表",
            IsSelected = true
        });

        // 添加配置中的类型
        if (configs != null)
        {
            foreach (var config in configs)
                ExportTypes.Add(new ExportTypeInfo
                {
                    TypeName = config.Model,
                    DisplayName = config.Title,
                    IsSelected = true
                });
        }
        else
        {
            // 默认添加一些常见类型
            var defaultTypes = new[]
            {
                ("BosRecord", "点焊记录"),
                ("SpotWeldRecord", "点焊记录"),
                ("ArcWeldRecord", "弧焊记录"),
                ("DceRecord", "螺柱焊记录"),
                ("ImlightRecord", "涂胶检测记录")
            };

            foreach (var (typeName, displayName) in defaultTypes)
                ExportTypes.Add(new ExportTypeInfo
                {
                    TypeName = typeName,
                    DisplayName = displayName,
                    IsSelected = false
                });
        }

        UpdateSelectionSummary();
    }

    /// <summary>
    ///     更新选中零件信息
    /// </summary>
    private void UpdateSelectedPartInfo()
    {
        HasSelectedPart = _selectedPart != null;
        if (_selectedPart != null)
        {
            SelectedPartInfo = $"零件码：{_selectedPart.SerialNo}";
            ExportSelected = true;
            ExportAll = false;
        }
        else
        {
            SelectedPartInfo = "未选择零件";
            ExportSelected = false;
            ExportAll = true;
        }
    }

    /// <summary>
    ///     更新选择统计
    /// </summary>
    private void UpdateSelectionSummary()
    {
        var selectedCount = ExportTypes.Count(t => t.IsSelected);
        var totalCount = ExportTypes.Count;
        SelectionSummary = $"已选择 {selectedCount} / {totalCount} 项";
    }

    /// <summary>
    ///     执行数据导出
    /// </summary>
    private async Task ExportDataAsync(string exportPath, List<ExportTypeInfo> selectedTypes,
        CancellationToken cancellationToken)
    {
        var totalSteps = selectedTypes.Count;
        var currentStep = 0;

        foreach (var type in selectedTypes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // 更新进度
            ExportProgress = (double)currentStep / totalSteps * 100;

            // 模拟导出过程
            await Task.Delay(1000, cancellationToken);

            // 创建Excel文件
            var fileName = $"{type.DisplayName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var filePath = Path.Combine(exportPath, fileName);

            // 这里应该实现实际的数据导出逻辑
            // 暂时创建一个空文件作为示例
            await File.WriteAllTextAsync(filePath, $"导出文件：{type.DisplayName}", cancellationToken);

            currentStep++;
        }

        ExportProgress = 100;
    }

    #endregion
}

/// <summary>
///     导出类型信息
/// </summary>
public partial class ExportTypeInfo : ObservableObject
{
    /// <summary>
    ///     显示名称
    /// </summary>
    [ObservableProperty] private string _displayName = string.Empty;

    /// <summary>
    ///     是否选中
    /// </summary>
    [ObservableProperty] private bool _isSelected;

    /// <summary>
    ///     类型名称
    /// </summary>
    [ObservableProperty] private string _typeName = string.Empty;

    partial void OnIsSelectedChanged(bool value)
    {
        // 当选择状态改变时，可以在这里处理相关逻辑
    }
}

/// <summary>
///     导出完成事件参数
/// </summary>
public class ExportCompletedEventArgs : EventArgs
{
    public ExportCompletedEventArgs(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public bool Success { get; }
    public string Message { get; }
}