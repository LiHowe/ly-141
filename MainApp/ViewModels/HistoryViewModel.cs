using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core;
using Core.Interfaces;
using Core.Models;
using Core.Models.Records;
using Core.Models.Settings;
using Core.Repositories;
using Core.Services;
using Core.Utils;
using Data.SqlSugar;
using Logger;
using MainApp.Controls;
using SqlSugar;
using UI.Controls;
using UI.ViewModels;
using MessageBox = UI.Controls.MessageBox;
using PlotViewModel = UI.ViewModels.PlotViewModel;

namespace MainApp.ViewModels;

/// <summary>
///     历史数据查询视图模型
/// </summary>
public partial class HistoryViewModel : ObservableObject
{
    #region 初始化方法

    /// <summary>
    ///     初始化默认值
    /// </summary>
    private void InitializeDefaultValues()
    {
        // 设置默认时间范围为今天
        var today = DateTime.Today;
        StartTime = today;
        EndTime = today.AddDays(1).AddSeconds(-1);

        AvailableQualityStatus.Add("全部");
        AvailableQualityStatus.Add("OK");
        AvailableQualityStatus.Add("NG");

        AvailableStations.Add("全部");

        // 设置默认质量状态
        QualityStatus = AvailableQualityStatus.First();
        Station = AvailableStations.First();
    }

    #endregion

    partial void OnSelectedPartChanged(ProductRecord? value)
    {
        SelectedPartSerialNo = value?.SerialNo ?? string.Empty;
    }

    #region 私有字段

    private readonly ISqlSugarClient? _dbClient;
    private ProductRepository _productRepository;
    private HistoryDetailSettings? _historySettings;

    private static readonly Assembly TargetAssembly = Assembly.GetAssembly(typeof(RecordBase));

    public static readonly Dictionary<string, Type> TypeMap = TargetAssembly
        .GetTypes()
        .Where(t => t.IsClass
                    && !t.IsAbstract
                    && t.Namespace == "Core.Models.Records"
                    && typeof(RecordBase).IsAssignableFrom(t))
        .ToDictionary(t => t.Name, t => t);

    #endregion

    #region 搜索条件属性

    /// <summary>
    ///     零件码/VIN码搜索文本
    /// </summary>
    [ObservableProperty] private string? _vinText;

    /// <summary>
    ///     搜索开始时间
    /// </summary>
    [ObservableProperty] private DateTime? _startTime;

    /// <summary>
    ///     搜索结束时间
    /// </summary>
    [ObservableProperty] private DateTime? _endTime;

    /// <summary>
    ///     选中的工作站
    /// </summary>
    [ObservableProperty] private string? _station;

    /// <summary>
    ///     选中的质量状态
    /// </summary>
    [ObservableProperty] private string? _qualityStatus;

    /// <summary>
    ///     工作站列表
    /// </summary>
    public ObservableCollection<string> Workstations { get; set; } = new() { "全部" };

    #endregion

    #region 数据显示属性

    [ObservableProperty] private ObservableCollection<string> _availableQualityStatus = new();
    [ObservableProperty] private ObservableCollection<string> _availableStations = new();

    /// <summary>
    ///     零件列表数据
    /// </summary>
    [ObservableProperty] private ObservableCollection<ProductRecord> _partsList = new();

    /// <summary>
    ///     选中的零件项
    /// </summary>
    [ObservableProperty] private ProductRecord? _selectedPart;

    /// <summary>
    ///     当前选中零件的零件码
    /// </summary>
    [ObservableProperty] private string _SelectedPartSerialNo = string.Empty;

    /// <summary>
    ///     历史详细数据
    /// </summary>
    [ObservableProperty] private DataTable? _historyDetailData;

    /// <summary>
    ///     统计信息文本
    /// </summary>
    [ObservableProperty] private string _statistics = "暂无统计数据";

    /// <summary>
    ///     是否显示图表
    /// </summary>
    [ObservableProperty] private Visibility _hasPlot = Visibility.Collapsed;

    /// <summary>
    ///     历史详情标签页列表
    /// </summary>
    [ObservableProperty] private ObservableCollection<HistoryDetailTab> _historyDetailTabs = new();

    /// <summary>
    ///     当前选中的历史详情标签页
    /// </summary>
    [ObservableProperty] private HistoryDetailTab? _selectedHistoryDetailTab;

    public bool IsSelected(HistoryDetailTab tab)
    {
        return SelectedHistoryDetailTab == tab;
    }

    /// <summary>
    ///     详情页的图表
    /// </summary>
    [ObservableProperty] private ObservableCollection<PlotViewModel> _historyDetailPlots = new();

    #endregion

    #region 分页属性

    /// <summary>
    ///     当前页码
    /// </summary>
    [ObservableProperty] private int _currentPage = 1;

    /// <summary>
    ///     每页大小
    /// </summary>
    [ObservableProperty] private int _pageSize = 20;

    /// <summary>
    ///     总页数
    /// </summary>
    [ObservableProperty] private int _totalPages = 1;

    /// <summary>
    ///     总记录数
    /// </summary>
    [ObservableProperty] private int _totalCount;

    /// <summary>
    ///     是否正在加载数据
    /// </summary>
    [ObservableProperty] private bool _isLoading;

    /// <summary>
    /// 正在加载详情数据
    /// </summary>
    [ObservableProperty] private bool _isLoadingDetail;

    #endregion

    #region 构造函数

    public HistoryViewModel()
    {
        var dbconfig = ConfigManager.Instance.LoadConfig<DatabaseSettings>(Constants.LocalDbConfigFilePath);
        ConfigManager.Instance.ConfigChanged += (sender, args) =>
        {
            if (args.ConfigType == ConfigType.Database)
            {
                Log.Info("数据库配置变更，重新加载数据库");
                dbconfig = ConfigManager.Instance.LoadConfig<DatabaseSettings>(Constants.LocalDbConfigFilePath, false);
                var sugar = new Sugar(dbconfig.ToSugarConfig());
                _productRepository = new ProductRepository(sugar);
            }
        };
        if (dbconfig == null)
        {
            Log.Error("获取本地数据库配置失败");
            throw new ConfigurationErrorsException("获取本地数据库配置失败");
        }

        var sugar = new Sugar(dbconfig.ToSugarConfig());
        _productRepository = new ProductRepository(sugar);
        _dbClient = sugar.GetDb();
        InitializeDefaultValues();

        // 加载历史详情配置
        LoadHistoryDetailSettings();

        // 监听选中零件变化
        PropertyChanged += OnPropertyChanged;
    }

    public HistoryViewModel(ISqlSugarClient dbClient) : this()
    {
        _dbClient = dbClient;
    }

    #endregion

    #region 属性变化处理

    /// <summary>
    ///     属性变化处理
    /// </summary>
    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedPart))
        {
            await LoadHistoryDetailTabsAsync();
            await LoadPartDetailAsync();
            await LoadPartPlotAsync();
        }
        else if (e.PropertyName == nameof(CurrentPage))
        {
            await SearchAsync();
        }
        else if (e.PropertyName == nameof(SelectedHistoryDetailTab))
        {
            await LoadPartDetailAsync();
        }
    }

    /// <summary>
    ///     加载图表数据
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private async Task LoadPartPlotAsync()
    {
        // 1. 获取配置文件
        if (SelectedHistoryDetailTab == null || _historySettings == null) return;
        var config = _historySettings.Configs.First(x => x.Title == SelectedHistoryDetailTab.Title);
        if (!config.ShowPlot) return;

        // 2. 清空原有图表
        HistoryDetailPlots.Clear();
        if (HistoryDetailData == null) return;
        if (HistoryDetailData.Rows.Count == 0) return;
        // 3. 根据配置加载图表数据
        foreach (var plot in config.Plots)
        {
            PlotViewModel vm = new();
            vm.Title = plot.Title;
            vm.XLabel = plot.XLabel;
            vm.YLabel = plot.YLabel;
            var dataSeries = new ObservableCollection<DataSeries>();
            vm.DataSeries = dataSeries;

            // 遍历配置列
            foreach (var colName in plot.Columns)
            {
                DataSeries series = new();

                var groupColumnName = plot.GroupColumnName;
                series.Legend = colName;
                // 数据分组
                if (!string.IsNullOrWhiteSpace(groupColumnName))
                {
                    var groupedData = HistoryDetailData.Rows.Cast<DataRow>()
                        .GroupBy(row => row.Field<string>(plot.GroupColumnName));
                    foreach (var group in groupedData)
                    {
                        ObservableCollection<double> values = new();
                        foreach (var row in group) values.Add(Convert.ToDouble(row[colName]));
                        series.DataY = values;
                        series.Legend = groupColumnName;
                        dataSeries.Add(series);
                    }
                }
                else
                {
                    ObservableCollection<double> values = new();
                    // 获取 DataTable 中的所有数据
                    foreach (DataRow row in HistoryDetailData.Rows)
                        if (HistoryDetailData.Columns.Contains(colName))
                        {
                            var val = row[colName];
                            values.Add(Convert.ToDouble(row[colName]));
                        }

                    series.Legend = colName;
                    series.DataY = values;
                    dataSeries.Add(series);
                }
            }

            HistoryDetailPlots.Add(vm);
        }
    }

    #endregion

    #region 命令实现

    /// <summary>
    ///     搜索命令
    /// </summary>
    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            IsLoading = true;

            // 构建查询条件
            var query = BuildSearchQuery();

            // 执行分页查询
            var result = await QueryHistoryDataAsync(query);

            // 更新UI数据
            UpdateSearchResults(result);
        }
        catch (Exception ex)
        {
            MessageBox.Error($"搜索失败：{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     导出命令
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            // 创建导出选项控件
            var exportOption = new HistoryExportOption(SelectedPart,
                _historySettings?.Configs ?? new List<HistoryDetailConfig>());

            // 创建Shell窗口
            var shellWindow = new ShellWindow("历史数据导出选项", "📤");
            shellWindow.Title = "导出选项";
            shellWindow.Height = 600;
            shellWindow.Width = 500;
            shellWindow.SetContent(exportOption);

            // 处理导出完成事件
            exportOption.ExportCompleted += (sender, args) =>
            {
                shellWindow.Close();
                if (args.Success)
                    MessageBox.Success("导出成功！");
                else
                    MessageBox.Error($"导出失败：{args.Message}");
            };

            // 处理取消事件
            exportOption.Cancelled += (sender, args) => { shellWindow.Close(); };

            shellWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Error($"打开导出窗口失败：{ex.Message}");
        }
    }

    /// <summary>
    ///     重置命令
    /// </summary>
    [RelayCommand]
    private void Reset()
    {
        VinText = string.Empty;
        Station = "全部";
        QualityStatus = "全部";

        // 重置时间为今天
        var today = DateTime.Today;
        StartTime = today;
        EndTime = today.AddDays(1).AddSeconds(-1);

        // 重置分页
        CurrentPage = 1;

        // 清空数据
        PartsList.Clear();
        HistoryDetailData = null;
        Statistics = "暂无统计数据";
        HasPlot = Visibility.Collapsed;
        HistoryDetailTabs.Clear();
        SelectedHistoryDetailTab = null;
    }

    /// <summary>
    ///     选择历史详情标签页命令
    /// </summary>
    [RelayCommand]
    private void SelectHistoryDetailTab(HistoryDetailTab tab)
    {
        SelectedHistoryDetailTab = tab;
		foreach (var item in HistoryDetailTabs)
		{
            item.IsSelected = false;
		}
		tab.IsSelected = true;
        LoadPartPlotAsync();

	}

    #endregion

    #region 数据查询方法

    /// <summary>
    ///     构建搜索查询条件
    /// </summary>
    private RecordQueryBase BuildSearchQuery()
    {
        return new RecordQueryBase
        {
            SerialNo = string.IsNullOrWhiteSpace(VinText) ? null : VinText.Trim(),
            StartTime = StartTime,
            EndTime = EndTime,
            Quality = QualityStatus == "全部" ? null : QualityStatus,
            CurrentPage = CurrentPage,
            Station = Station == "全部" ? null : Station,
            PageSize = PageSize,
            Remark = Station == "全部" ? null : Station
        };
    }

    /// <summary>
    ///     查询历史数据
    /// </summary>
    private async Task<PagedList<ProductRecord>> QueryHistoryDataAsync(RecordQueryBase query)
    {
        try
        {
            return await _productRepository.GetPagedAsync(query);
        }
        catch (Exception ex)
        {
            throw new Exception($"查询历史数据失败：{ex.Message}", ex);
        }
    }

    /// <summary>
    ///     更新搜索结果
    /// </summary>
    private void UpdateSearchResults(PagedList<ProductRecord> result)
    {
        PartsList.Clear();
        foreach (var item in result.Data) PartsList.Add(item);

        TotalCount = result.TotalCount;
        TotalPages = (int)Math.Ceiling((double)result.TotalCount / result.PageSize);
        CurrentPage = result.CurrentPage;
    }

    #endregion

    #region 详细数据处理

    /// <summary>
    ///     加载零件详细数据
    /// </summary>
    private async Task LoadPartDetailAsync()
    {
        if (SelectedPart == null)
        {
            HistoryDetailData = null;
            Statistics = "暂无统计数据";
            HasPlot = Visibility.Collapsed;
            return;
        }

        // 获取选中的标签页来加载对应的详情数据
        if (SelectedHistoryDetailTab == null) return;

        try
        {
            IsLoadingDetail = true;

            var historyDetailConfig = _historySettings.Configs.First(x => x.Title == SelectedHistoryDetailTab.Title);

            // 根据配置的Model来反射模型类型，然后查询数据
            var modelType = TypeMap[historyDetailConfig.Model];
            if (modelType == null)
            {
                MessageBox.Error($"未找到模型类型：{historyDetailConfig.Model}");
                return;
            }

            // 判断是否需要显示图表
            HasPlot = ShouldShowPlot() ? Visibility.Visible : Visibility.Collapsed;

            // 加载图表
            if (HasPlot == Visibility.Visible) await LoadPartPlotAsync();

            var entityInfo = _dbClient.EntityMaintenance.GetEntityInfo(modelType);

            var colBuilder = new StringBuilder();
            if (historyDetailConfig.Columns.Count > 0)
            {
                // 优先使用Columns定义
                foreach (var col in entityInfo.Columns)
                    if (historyDetailConfig.Columns.Contains(col.DbColumnName) ||
                        historyDetailConfig.Columns.Contains(col.PropertyName))
                        colBuilder
                            .Append(col.DbColumnName)
                            .Append(" as ")
                            .Append(col.ColumnDescription)
                            .Append(",");
            }
            else if (historyDetailConfig.HiddenColumns.Count > 0)
            {
                // 使用HiddenColumns隐藏列
                foreach (var col in entityInfo.Columns)
                    if (!historyDetailConfig.HiddenColumns.Contains(col.DbColumnName) ||
                        !historyDetailConfig.HiddenColumns.Contains(col.PropertyName))
                        colBuilder.Append(col.DbColumnName).Append(",");
            }
            else
            {
                // 均无配置，全都展示
                // 优先使用Columns定义
                foreach (var col in entityInfo.Columns)
                    colBuilder
                        .Append(col.DbColumnName)
                        .Append(" as ")
                        .Append(col.ColumnDescription)
                        .Append(",");
            }

            var columnString = colBuilder.ToString().TrimEnd(',');

            // 根据表信息以及历史配置来构建查询语句
            var sql =
                $"SELECT {columnString} FROM {entityInfo.DbTableName} WHERE serial_no = '{SelectedPart.SerialNo}'";

            // 执行查询
            var dataTable = _dbClient.Ado.GetDataTable(sql);
            HistoryDetailData = dataTable;
        }
        catch (Exception ex)
        {
            MessageBox.Error($"加载详细数据失败：{ex.Message}");
        }
        finally
        {
            IsLoadingDetail = false;
        }
    }

    /// <summary>
    ///     判断是否应该显示图表
    /// </summary>
    private bool ShouldShowPlot()
    {
        if (SelectedHistoryDetailTab == null) return false;
        if (HistoryDetailData == null) return false;
        if (_historySettings == null) return false;
        var historyDetailConfig = _historySettings.Configs.First(x => x.Title == SelectedHistoryDetailTab.Title);
        return historyDetailConfig.ShowPlot;
    }

    #endregion

    #region 数据导出方法

    /// <summary>
    ///     导出到Excel文件
    /// </summary>
    private async Task ExportToExcelAsync(List<ProductRecord> data, string filePath)
    {
        await Task.Run(() =>
        {
            try
            {
                // 这里应该使用Excel导出库，如EPPlus或NPOI
                // 由于项目中可能没有这些库，这里提供一个简单的CSV格式导出
                var csvContent = new StringBuilder();
                csvContent.AppendLine("零件码,创建时间,质量状态");

                foreach (var item in data)
                    csvContent.AppendLine($"{item.SerialNo},{item.CreateTime:yyyy-MM-dd HH:mm:ss},{item.Quality}");

                File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Excel导出失败：{ex.Message}", ex);
            }
        });
    }

    /// <summary>
    ///     导出到CSV文件
    /// </summary>
    private async Task ExportToCsvAsync(List<ProductRecord> data, string filePath)
    {
        await Task.Run(() =>
        {
            try
            {
                var csvContent = new StringBuilder();
                csvContent.AppendLine("零件码,创建时间,质量状态");

                foreach (var item in data)
                    csvContent.AppendLine($"{item.SerialNo},{item.CreateTime:yyyy-MM-dd HH:mm:ss},{item.Quality}");

                File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"CSV导出失败：{ex.Message}", ex);
            }
        });
    }

    #endregion

    #region 公共方法

    /// <summary>
    ///     刷新数据
    /// </summary>
    public async Task RefreshAsync()
    {
        await SearchAsync();
    }

    /// <summary>
    ///     页码变化处理（由View调用）
    /// </summary>
    public async Task OnPageChangedAsync(int newPageIndex)
    {
        if (newPageIndex != CurrentPage && newPageIndex > 0 && newPageIndex <= TotalPages)
        {
            CurrentPage = newPageIndex;
            await SearchAsync();
        }
    }

    /// <summary>
    ///     加载历史详情配置
    /// </summary>
    private void LoadHistoryDetailSettings()
    {
        try
        {
            _historySettings = ConfigManager.Instance.LoadConfig<HistoryDetailSettings>(Constants.HisConfigFilePath);
            if (_historySettings == null) _historySettings = new HistoryDetailSettings();
        }
        catch (Exception ex)
        {
            Log.Error($"加载历史详情配置失败：{ex.Message}");
            _historySettings = new HistoryDetailSettings();
        }
    }

    /// <summary>
    ///     根据选中零件加载历史详情标签页
    /// </summary>
    private async Task LoadHistoryDetailTabsAsync()
    {
        IsLoadingDetail = true;
        // 1. 清空历史详情标签页
        HistoryDetailTabs.Clear();
        
        // 2. 加载历史详情配置
        if (SelectedPart == null || _historySettings == null) return;

        // 3. 加载历史详情标签页
        foreach (var config in _historySettings.Configs)
            try
            {
                // 查询该类型的数据总数
                var count = await GetHistoryDetailCountAsync(config.Model, SelectedPart.SerialNo);

                if (count > 0)
                {
                    var tab = new HistoryDetailTab
                    {
                        Config = config,
                        Title = config.Title,
                        DataCount = count,
                        IsVisible = true,
                        IsSelected = false,
                    };

                    HistoryDetailTabs.Add(tab);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"加载历史详情标签页失败 - {config.Title}：{ex.Message}");
            }

        // 选中第一个标签页
        var firstTab = HistoryDetailTabs.FirstOrDefault();
        if (firstTab is null) return;
        firstTab.IsSelected = true;
		SelectedHistoryDetailTab = firstTab;
    }

    /// <summary>
    ///     获取历史详情数据总数
    /// </summary>
    private async Task<int> GetHistoryDetailCountAsync(string modelName, string? serialNo)
    {
        if (_dbClient == null || string.IsNullOrEmpty(serialNo)) return 0;

        try
        {
            var historyDetailConfig = _historySettings.Configs.First(x => x.Model == modelName);

            // 根据配置的Model来反射模型类型，然后查询数据
            // var modelType = Type.GetType($"Core.Models.Records.{historyDetailConfig.Model}", true, true);
            var modelType = TypeMap[historyDetailConfig.Model];
            if (modelType == null)
            {
                MessageBox.Error($"未找到模型类型：{historyDetailConfig.Model}");
                return 0;
            }

            var entityInfo = _dbClient.EntityMaintenance.GetEntityInfo(modelType);
            var sql = $"SELECT count(0) FROM {entityInfo.DbTableName} WHERE serial_no = '{SelectedPart.SerialNo}'";

            // 执行查询
            var res = await _dbClient.Ado.GetIntAsync(sql);
            return res;
        }
        catch (Exception ex)
        {
            Log.Error($"查询历史详情数据总数失败 - {modelName}：{ex.Message}");
            return 0;
        }
    }

    #endregion
}