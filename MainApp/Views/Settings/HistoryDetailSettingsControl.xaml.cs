using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core;
using Core.Interfaces;
using Core.Models;
using Core.Models.Settings;
using Core.Utils;
using Microsoft.Win32;
using SqlSugar;
using ValidationResult = Core.Interfaces.ValidationResult;
using MessageBox = UI.Controls.MessageBox;

namespace MainApp.Views.Settings;

/// <summary>
///     历史详情设置控件
/// </summary>
public partial class HistoryDetailSettingsControl : UserControl, ISettingsControl
{
    private readonly HistoryDetailSettingsViewModel _viewModel;

    public HistoryDetailSettingsControl()
    {
        InitializeComponent();
        _viewModel = new HistoryDetailSettingsViewModel();
        DataContext = _viewModel;
    }

    public string CategoryName => "HistoryDetail";
    public string DisplayTitle => "历史查询配置";
    public bool HasUnsavedChanges => _viewModel.HasUnsavedChanges;

    public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

    public async Task InitializeAsync(IConfigManager configManager)
    {
        // 初始化逻辑
        await Task.CompletedTask;
    }

    public async Task LoadSettingsAsync()
    {
        await Task.CompletedTask;
    }

    public async Task SaveSettingsAsync()
    {
        await _viewModel.SaveAsync();
    }

    public ValidationResult ValidateSettings()
    {
        return _viewModel.ValidateSettings();
    }

    public async Task ResetToDefaultAsync()
    {
        _viewModel.Reset();
        await Task.CompletedTask;
    }

    public async Task RefreshAsync()
    {
        _viewModel.Reset();
        await Task.CompletedTask;
    }

    /// <summary>
    ///     模型选择变化事件处理
    /// </summary>
    private void OnModelSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedModel)
            _viewModel.OnModelChanged(selectedModel);
    }
}

/// <summary>
///     历史详情设置视图模型
/// </summary>
public partial class HistoryDetailSettingsViewModel : ObservableObject
{
    private bool _hasUnsavedChanges;
    private HistoryDetailSettings _settings;

    public HistoryDetailSettingsViewModel()
    {
        LoadSettings();
        InitializeAvailableModels();
        AvailableOrderBys.Add("asc");
        AvailableOrderBys.Add("desc");
    }

    #region 属性

    /// <summary>
    ///     配置列表
    /// </summary>
    [ObservableProperty] private ObservableCollection<HistoryDetailConfig> _configs = new();

    /// <summary>
    ///     选中的配置
    /// </summary>
    [ObservableProperty] private HistoryDetailConfig? _selectedConfig;

    /// <summary>
    ///     是否有选中的配置
    /// </summary>
    [ObservableProperty] private bool _hasSelectedConfig;

    /// <summary>
    ///     可用的模型列表
    /// </summary>
    [ObservableProperty] private ObservableCollection<string> _availableModels = new();

    [ObservableProperty] private ObservableCollection<string> _availableOrderBys = new();

    /// <summary>
    ///     模型列信息
    /// </summary>
    [ObservableProperty] private ObservableCollection<ModelColumnInfo> _modelColumns = new();

    /// <summary>
    ///     是否显示指定列（true）还是隐藏指定列（false）
    /// </summary>
    [ObservableProperty] private bool _isShowSpecificColumns = true;

    /// <summary>
    ///     列模式描述
    /// </summary>
    [ObservableProperty] private string _columnModeDescription = "选择要显示的列";

    /// <summary>
    ///     是否有未保存的更改
    /// </summary>
    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set => SetProperty(ref _hasUnsavedChanges, value);
    }

    /// <summary>
    ///     选中的图表定义
    /// </summary>
    [ObservableProperty] private PlotDefinition? _selectedPlot;

    /// <summary>
    ///     是否有选中的图表
    /// </summary>
    [ObservableProperty] private bool _hasSelectedPlot;

    /// <summary>
    ///     模型列名称列表（用于下拉框）
    /// </summary>
    [ObservableProperty] private ObservableCollection<string> _modelColumnNames = new();

    /// <summary>
    ///     图表列信息
    /// </summary>
    [ObservableProperty] private ObservableCollection<ModelColumnInfo> _plotColumns = new();

    #endregion

    #region 命令

    /// <summary>
    ///     添加配置命令
    /// </summary>
    [RelayCommand]
    private void AddConfig()
    {
        var newConfig = new HistoryDetailConfig
        {
            Title = $"新配置 {Configs.Count + 1}",
            Model = AvailableModels.FirstOrDefault() ?? string.Empty
        };

        Configs.Add(newConfig);
        SelectedConfig = newConfig;
        HasUnsavedChanges = true;
    }

    /// <summary>
    ///     删除配置命令
    /// </summary>
    [RelayCommand]
    private void DeleteConfig(HistoryDetailConfig config)
    {
        if (config == null) return;

        var result = MessageBox.Question(
            $"确定要删除配置 '{config.Title}' 吗？");

        if (result == MessageBoxResult.Yes)
        {
            Configs.Remove(config);
            if (SelectedConfig == config) SelectedConfig = Configs.FirstOrDefault();
            HasUnsavedChanges = true;
        }
    }

    /// <summary>
    ///     保存命令
    /// </summary>
    [RelayCommand]
    private async Task Save()
    {
        await SaveAsync();
    }

    /// <summary>
    ///     添加图表命令
    /// </summary>
    [RelayCommand]
    private void AddPlot()
    {
        if (SelectedConfig == null) return;

        var newPlot = new PlotDefinition
        {
            Title = $"图表 {SelectedConfig.Plots.Count + 1}",
            XLabel = "时间",
            YLabel = "数值"
        };

        SelectedConfig.Plots.Add(newPlot);
        SelectedPlot = newPlot;
        HasUnsavedChanges = true;
    }

    /// <summary>
    ///     删除图表命令
    /// </summary>
    [RelayCommand]
    private void DeletePlot(PlotDefinition plot)
    {
        if (SelectedConfig == null || plot == null) return;

        var result = MessageBox.Question(
            $"确定要删除图表 '{plot.Title}' 吗？");

        if (result == MessageBoxResult.Yes)
        {
            SelectedConfig.Plots.Remove(plot);
            if (SelectedPlot == plot) SelectedPlot = SelectedConfig.Plots.FirstOrDefault();
            HasUnsavedChanges = true;
        }
    }

    /// <summary>
    ///     浏览文件夹命令
    /// </summary>
    [RelayCommand]
    private void BrowseFolder()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择图片文件夹",
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "选择文件夹"
        };

        if (dialog.ShowDialog() == true)
        {
            var folderPath = Path.GetDirectoryName(dialog.FileName);
            if (SelectedConfig?.Image != null && !string.IsNullOrEmpty(folderPath))
            {
                SelectedConfig.Image.FolderPath = folderPath;
                HasUnsavedChanges = true;
            }
        }
    }

    #endregion

    #region 属性变化处理

    partial void OnSelectedConfigChanged(HistoryDetailConfig? value)
    {
        HasSelectedConfig = value != null;
        if (value != null) LoadModelColumns(value.Model);
    }

    partial void OnIsShowSpecificColumnsChanged(bool value)
    {
        ColumnModeDescription = value ? "选择要显示的列" : "选择要隐藏的列";
        UpdateColumnSelection();
    }

    partial void OnSelectedPlotChanged(PlotDefinition? value)
    {
        HasSelectedPlot = value != null;
        if (value != null) LoadPlotColumns();
    }

    #endregion

    #region 私有方法

    /// <summary>
    ///     加载设置
    /// </summary>
    private void LoadSettings()
    {
        try
        {
            _settings = ConfigManager.Instance.LoadConfig<HistoryDetailSettings>(Constants.HisConfigFilePath)
                        ?? new HistoryDetailSettings();

            Configs.Clear();
            foreach (var config in _settings.Configs) Configs.Add(config);

            SelectedConfig = Configs.FirstOrDefault();
        }
        catch (Exception ex)
        {
            MessageBox.Error($"加载配置失败：{ex.Message}");
            _settings = new HistoryDetailSettings();
        }
    }

    /// <summary>
    ///     初始化可用模型列表
    /// </summary>
    private void InitializeAvailableModels()
    {
        try
        {
            var assembly = Assembly.GetAssembly(typeof(RecordBase));
            if (assembly == null) return;

            var recordTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(RecordBase).IsAssignableFrom(t))
                .Select(t => t.Name)
                .OrderBy(name => name)
                .ToList();

            // 添加MainApp程序集中的ProductRecord
            recordTypes.Add("ProductRecord");

            AvailableModels.Clear();
            foreach (var type in recordTypes) AvailableModels.Add(type);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载模型列表失败：{ex.Message}");
        }
    }

    /// <summary>
    ///     加载模型列信息
    /// </summary>
    private void LoadModelColumns(string modelName)
    {
        ModelColumns.Clear();
        ModelColumnNames.Clear();

        if (string.IsNullOrEmpty(modelName)) return;

        try
        {
            Type? modelType = null;

            // 先在Core程序集中查找
            var coreAssembly = Assembly.GetAssembly(typeof(RecordBase));
            modelType = coreAssembly?.GetTypes()
                .FirstOrDefault(t => t.Name == modelName && typeof(RecordBase).IsAssignableFrom(t));

            // 如果没找到，在MainApp程序集中查找
            if (modelType == null)
            {
                var mainAssembly = Assembly.GetExecutingAssembly();
                modelType = mainAssembly.GetTypes()
                    .FirstOrDefault(t => t.Name == modelName && typeof(RecordBase).IsAssignableFrom(t));
            }

            if (modelType == null) return;

            var properties = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                var sugarColumn = prop.GetCustomAttribute<SugarColumn>();
                var displayName = sugarColumn?.ColumnDescription ?? prop.Name;

                var columnInfo = new ModelColumnInfo
                {
                    PropertyName = prop.Name,
                    DisplayName = displayName,
                    IsSelected = true // 默认全选
                };

                ModelColumns.Add(columnInfo);
                ModelColumnNames.Add(prop.Name);
            }

            UpdateColumnSelection();
            LoadPlotColumns();
        }
        catch (Exception ex)
        {
            MessageBox.Error($"加载模型列信息失败：{ex.Message}");
        }
    }

    /// <summary>
    ///     更新列选择状态
    /// </summary>
    private void UpdateColumnSelection()
    {
        if (SelectedConfig == null) return;

        foreach (var column in ModelColumns)
            if (IsShowSpecificColumns)
                // 显示模式：如果在Columns列表中则选中
                column.IsSelected = SelectedConfig.Columns.Contains(column.PropertyName) ||
                                    SelectedConfig.Columns.Contains(column.DisplayName);
            else
                // 隐藏模式：如果在HiddenColumns列表中则不选中
                column.IsSelected = !SelectedConfig.HiddenColumns.Contains(column.PropertyName) &&
                                    !SelectedConfig.HiddenColumns.Contains(column.DisplayName);
    }

    #endregion

    #region 公共方法

    /// <summary>
    ///     保存设置
    /// </summary>
    public async Task<bool> SaveAsync()
    {
        try
        {
            // 更新选中配置的列设置
            UpdateSelectedConfigColumns();

            // 更新图表列设置
            UpdateSelectedPlotColumns();

            _settings.Configs.Clear();
            _settings.Configs.AddRange(Configs);

            await ConfigManager.Instance.SaveConfigAsync(Constants.HisConfigFilePath, _settings);
            HasUnsavedChanges = false;
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Error($"保存失败：{ex.Message}");
            return false;
        }
    }

    /// <summary>
    ///     更新选中图表的列设置
    /// </summary>
    private void UpdateSelectedPlotColumns()
    {
        if (SelectedPlot == null) return;

        SelectedPlot.Columns.Clear();
        foreach (var column in PlotColumns.Where(c => c.IsSelected))
            // 因为表格列已经根据表注释进行中文化，所以使用display名称进行显示
            SelectedPlot.Columns.Add(column.DisplayName);
    }

    /// <summary>
    ///     更新选中配置的列设置
    /// </summary>
    private void UpdateSelectedConfigColumns()
    {
        if (SelectedConfig == null) return;

        SelectedConfig.Columns.Clear();
        SelectedConfig.HiddenColumns.Clear();

        if (IsShowSpecificColumns)
            // 显示模式：将选中的列添加到Columns
            foreach (var column in ModelColumns.Where(c => c.IsSelected))
                SelectedConfig.Columns.Add(column.PropertyName);
        else
            // 隐藏模式：将未选中的列添加到HiddenColumns
            foreach (var column in ModelColumns.Where(c => !c.IsSelected))
                SelectedConfig.HiddenColumns.Add(column.PropertyName);
    }

    /// <summary>
    ///     加载图表列信息
    /// </summary>
    private void LoadPlotColumns()
    {
        PlotColumns.Clear();

        if (SelectedPlot == null) return;

        foreach (var column in ModelColumns)
        {
            var plotColumn = new ModelColumnInfo
            {
                PropertyName = column.PropertyName,
                DisplayName = column.DisplayName,
                IsSelected = SelectedPlot.Columns.Contains(column.DisplayName)
            };

            PlotColumns.Add(plotColumn);
        }
    }

    /// <summary>
    ///     模型变化处理
    /// </summary>
    public void OnModelChanged(string modelName)
    {
        if (SelectedConfig != null)
        {
            SelectedConfig.Model = modelName;
            LoadModelColumns(modelName);
            HasUnsavedChanges = true;
        }
    }

    /// <summary>
    ///     验证设置
    /// </summary>
    public ValidationResult ValidateSettings()
    {
        var result = new ValidationResult();

        foreach (var config in Configs)
        {
            if (string.IsNullOrWhiteSpace(config.Title)) result.AddError("配置标题不能为空");

            if (string.IsNullOrWhiteSpace(config.Model)) result.AddError("必须选择模型类");
        }

        return result;
    }

    /// <summary>
    ///     重置设置
    /// </summary>
    public void Reset()
    {
        LoadSettings();
        HasUnsavedChanges = false;
    }

    #endregion
}

/// <summary>
///     模型列信息
/// </summary>
public partial class ModelColumnInfo : ObservableObject
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
    ///     属性名
    /// </summary>
    [ObservableProperty] private string _propertyName = string.Empty;
}