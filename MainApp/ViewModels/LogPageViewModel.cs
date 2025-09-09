using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Models;
using Core.Services;
using Microsoft.Win32;

namespace MainApp.ViewModels;

/// <summary>
///     日志页面ViewModel
/// </summary>
public partial class LogPageViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherTimer _autoRefreshTimer;
    private readonly LogReaderService _logReaderService;
    private bool _disposed;
    private bool _isRefreshing;

    #region 构造函数

    /// <summary>
    ///     构造函数
    /// </summary>
    public LogPageViewModel()
    {
        _logReaderService = new LogReaderService();

        // 初始化自动刷新定时器
        _autoRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(AutoRefreshInterval)
        };
        _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

        // 监听属性变化
        PropertyChanged += OnPropertyChanged;

        // 初始化加载日志文件列表
        _ = LoadLogFilesAsync();
    }

    #endregion

    #region IDisposable

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _autoRefreshTimer?.Stop();
            if (_autoRefreshTimer != null)
                _autoRefreshTimer.Tick -= AutoRefreshTimer_Tick;
            PropertyChanged -= OnPropertyChanged;
            _disposed = true;
        }
    }

    #endregion

    #region 可观察属性

    /// <summary>
    ///     日志文件列表
    /// </summary>
    [ObservableProperty] private ObservableCollection<LogFileInfo> _logFiles = new();

    /// <summary>
    ///     选中的日志文件
    /// </summary>
    [ObservableProperty] private LogFileInfo? _selectedLogFile;

    /// <summary>
    ///     日志条目集合
    /// </summary>
    [ObservableProperty] private ObservableCollection<LogEntry> _logEntries = new();

    /// <summary>
    ///     过滤后的日志条目集合
    /// </summary>
    [ObservableProperty] private ObservableCollection<LogEntry> _filteredLogEntries = new();

    /// <summary>
    ///     选中的日志条目
    /// </summary>
    [ObservableProperty] private LogEntry? _selectedLogEntry;

    /// <summary>
    ///     搜索文本
    /// </summary>
    [ObservableProperty] private string _searchText = string.Empty;

    /// <summary>
    ///     选中的日志等级过滤
    /// </summary>
    [ObservableProperty] private LogLevel? _selectedLogLevel; // 默认为null表示"全部"

    /// <summary>
    ///     状态文本
    /// </summary>
    [ObservableProperty] private string _statusText = "就绪";

    /// <summary>
    ///     是否正在加载
    /// </summary>
    [ObservableProperty] private bool _isLoading;

    /// <summary>
    ///     是否启用自动刷新
    /// </summary>
    [ObservableProperty] private bool _isAutoRefreshEnabled; // 暂时禁用自动刷新

    /// <summary>
    ///     自动刷新间隔（秒）
    /// </summary>
    [ObservableProperty] private int _autoRefreshInterval = 30;

    /// <summary>
    ///     最大显示条目数
    /// </summary>
    [ObservableProperty] private int _maxDisplayEntries = 1000;

    /// <summary>
    ///     日志统计信息
    /// </summary>
    [ObservableProperty] private string _logStatistics = string.Empty;

    /// <summary>
    ///     最后更新时间
    /// </summary>
    [ObservableProperty] private string _lastUpdateTime = "从未更新";

    #endregion

    #region 命令

    /// <summary>
    ///     刷新日志命令
    /// </summary>
    // [RelayCommand(CanExecute = nameof(CanRefresh))]
    [RelayCommand]
    private async Task RefreshLogsAsync()
    {
        // 刷新文件列表
        await LoadLogFilesAsync();

        // 如果有选中的文件，重新加载其内容
        if (SelectedLogFile != null) await LoadLogsAsync();
    }

    /// <summary>
    ///     清空日志命令
    /// </summary>
    [RelayCommand]
    private void ClearLogs()
    {
        try
        {
            var result = MessageBox.Show("确定要清空当前显示的日志吗？", "确认清空",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LogEntries.Clear();
                FilteredLogEntries.Clear();
                SelectedLogEntry = null;
                UpdateStatistics();
                StatusText = "日志已清空";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"清空日志失败: {ex.Message}";
        }
    }

    /// <summary>
    ///     打开日志文件夹命令
    /// </summary>
    [RelayCommand]
    private void OpenLogDirectory()
    {
        try
        {
            _logReaderService.OpenLogDirectory();
            StatusText = "已打开日志文件夹";
        }
        catch (Exception ex)
        {
            StatusText = $"打开日志文件夹失败: {ex.Message}";
            MessageBox.Show($"打开日志文件夹失败: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    ///     应用过滤命令
    /// </summary>
    [RelayCommand]
    private void ApplyFilter()
    {
        try
        {
            // 确保在UI线程中执行
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => ApplyFilter());
                return;
            }

            var filtered = _logReaderService.FilterLogs(
                LogEntries.ToList(),
                SelectedLogLevel,
                SearchText);

            FilteredLogEntries.Clear();
            foreach (var entry in filtered) FilteredLogEntries.Add(entry);

            UpdateStatistics();
            StatusText = $"已应用过滤条件，显示 {FilteredLogEntries.Count} 条记录";
        }
        catch (Exception ex)
        {
            StatusText = $"应用过滤失败: {ex.Message}";
            Debug.WriteLine($"应用过滤失败: {ex.Message}");
        }
    }

    /// <summary>
    ///     重置过滤命令
    /// </summary>
    [RelayCommand]
    private void ResetFilter()
    {
        SearchText = string.Empty;
        SelectedLogLevel = null;
        ApplyFilter();
        StatusText = "已重置过滤条件";
    }

    /// <summary>
    ///     切换自动刷新命令
    /// </summary>
    [RelayCommand]
    private void ToggleAutoRefresh()
    {
        IsAutoRefreshEnabled = !IsAutoRefreshEnabled;
    }

    /// <summary>
    ///     删除日志文件命令
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteLogFile))]
    private void DeleteLogFile()
    {
        if (SelectedLogFile == null) return;

        try
        {
            var result = MessageBox.Show($"确定要删除日志文件 '{SelectedLogFile.FileName}' 吗？",
                "确认删除", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (_logReaderService.DeleteLogFile(SelectedLogFile.FilePath))
                {
                    StatusText = $"已删除日志文件: {SelectedLogFile.FileName}";
                    _ = LoadLogFilesAsync(); // 重新加载文件列表

                    // 清空当前显示的日志
                    LogEntries.Clear();
                    FilteredLogEntries.Clear();
                    SelectedLogEntry = null;
                    UpdateStatistics();
                }
                else
                {
                    StatusText = $"删除日志文件失败: {SelectedLogFile.FileName}";
                    MessageBox.Show($"删除日志文件失败: {SelectedLogFile.FileName}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            StatusText = $"删除日志文件失败: {ex.Message}";
            MessageBox.Show($"删除日志文件失败: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    ///     清空日志文件命令
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanClearLogFile))]
    private void ClearLogFile()
    {
        if (SelectedLogFile == null) return;

        try
        {
            var result = MessageBox.Show($"确定要清空日志文件 '{SelectedLogFile.FileName}' 的内容吗？",
                "确认清空", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (_logReaderService.ClearLogFile(SelectedLogFile.FilePath))
                {
                    StatusText = $"已清空日志文件: {SelectedLogFile.FileName}";

                    // 清空当前显示的日志
                    LogEntries.Clear();
                    FilteredLogEntries.Clear();
                    SelectedLogEntry = null;
                    UpdateStatistics();

                    // 重新加载文件列表以更新文件大小等信息
                    _ = LoadLogFilesAsync();
                }
                else
                {
                    StatusText = $"清空日志文件失败: {SelectedLogFile.FileName}";
                    MessageBox.Show($"清空日志文件失败: {SelectedLogFile.FileName}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            StatusText = $"清空日志文件失败: {ex.Message}";
            MessageBox.Show($"清空日志文件失败: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    ///     是否可以刷新
    /// </summary>
    private bool CanRefresh => !IsLoading;

    /// <summary>
    ///     是否可以删除日志文件
    /// </summary>
    private bool CanDeleteLogFile => SelectedLogFile != null && !IsLoading;

    /// <summary>
    ///     是否可以清空日志文件
    /// </summary>
    private bool CanClearLogFile => SelectedLogFile != null && !IsLoading;

    /// <summary>
    ///     加载日志文件列表
    /// </summary>
    private async Task LoadLogFilesAsync()
    {
        try
        {
            IsLoading = true;
            StatusText = "正在加载日志文件列表...";

            await Task.Run(() =>
            {
                var logFiles = _logReaderService.GetLogFiles();
                var fileInfos = new List<LogFileInfo>();

                foreach (var file in logFiles)
                {
                    var fileInfo = _logReaderService.GetLogFileInfo(file.FullName);
                    if (fileInfo != null) fileInfos.Add(fileInfo);
                }

                // 在UI线程中更新集合
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        LogFiles.Clear();
                        foreach (var fileInfo in fileInfos.OrderByDescending(f => f.LastModified))
                            LogFiles.Add(fileInfo);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"更新文件列表失败: {ex.Message}");
                    }
                });
            });

            StatusText = $"已加载 {LogFiles.Count} 个日志文件";
        }
        catch (Exception ex)
        {
            StatusText = $"加载日志文件列表失败: {ex.Message}";
            Debug.WriteLine($"LoadLogFilesAsync 异常: {ex}");
            MessageBox.Show($"加载日志文件列表失败: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     加载指定文件的日志
    /// </summary>
    private async Task LoadLogsAsync()
    {
        if (SelectedLogFile == null)
        {
            // 清空日志显示
            LogEntries.Clear();
            FilteredLogEntries.Clear();
            SelectedLogEntry = null;
            UpdateStatistics();
            StatusText = "请选择一个日志文件";
            return;
        }

        try
        {
            IsLoading = true;
            StatusText = $"正在加载日志文件: {SelectedLogFile.FileName}...";

            var logs = await _logReaderService.ReadLogFileAsync(SelectedLogFile.FilePath);

            // 限制显示条目数
            if (logs.Count > MaxDisplayEntries)
                logs = logs.OrderByDescending(l => l.Timestamp).Take(MaxDisplayEntries).ToList();

            // 在UI线程中更新集合
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    LogEntries.Clear();
                    foreach (var log in logs.OrderByDescending(l => l.Timestamp)) LogEntries.Add(log);

                    // 应用当前过滤条件
                    ApplyFilter();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"更新日志列表失败: {ex.Message}");
                }
            });

            LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
            StatusText = $"已加载 {logs.Count} 条日志记录 - {SelectedLogFile.FileName}";
        }
        catch (Exception ex)
        {
            StatusText = $"加载日志失败: {ex.Message}";
            MessageBox.Show($"加载日志失败: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    ///     更新统计信息
    /// </summary>
    private void UpdateStatistics()
    {
        if (FilteredLogEntries.Count == 0)
        {
            LogStatistics = "无日志记录";
            return;
        }

        var debugCount = FilteredLogEntries.Count(e => e.Level == LogLevel.Debug);
        var infoCount = FilteredLogEntries.Count(e => e.Level == LogLevel.Info);
        var warningCount = FilteredLogEntries.Count(e => e.Level == LogLevel.Warning);
        var errorCount = FilteredLogEntries.Count(e => e.Level == LogLevel.Error);

        LogStatistics = $"总计: {FilteredLogEntries.Count} | " +
                        $"调试: {debugCount} | " +
                        $"信息: {infoCount} | " +
                        $"警告: {warningCount} | " +
                        $"错误: {errorCount}";
    }

    /// <summary>
    ///     属性变化处理
    /// </summary>
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            switch (e.PropertyName)
            {
                case nameof(IsAutoRefreshEnabled):
                    UpdateAutoRefreshTimer();
                    break;
                case nameof(AutoRefreshInterval):
                    UpdateAutoRefreshTimer();
                    break;
                case nameof(SearchText):
                case nameof(SelectedLogLevel):
                    // 延迟应用过滤，避免频繁更新
                    Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        try
                        {
                            ApplyFilter();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"延迟过滤失败: {ex.Message}");
                        }
                    }, DispatcherPriority.Background);
                    break;
                case nameof(SelectedLogFile):
                    // 当选择的日志文件改变时，加载该文件的日志
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await LoadLogsAsync();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"加载日志文件失败: {ex.Message}");
                        }
                    });
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"属性变化处理失败: {ex.Message}");
        }
    }

    /// <summary>
    ///     更新自动刷新定时器
    /// </summary>
    private void UpdateAutoRefreshTimer()
    {
        _autoRefreshTimer.Stop();

        if (IsAutoRefreshEnabled && AutoRefreshInterval > 0)
        {
            _autoRefreshTimer.Interval = TimeSpan.FromSeconds(AutoRefreshInterval);
            _autoRefreshTimer.Start();
        }
    }

    /// <summary>
    ///     自动刷新定时器事件
    /// </summary>
    private async void AutoRefreshTimer_Tick(object? sender, EventArgs e)
    {
        try
        {
            if (!IsLoading && !_isRefreshing)
            {
                _isRefreshing = true;

                // 刷新文件列表
                await LoadLogFilesAsync();

                // 如果有选中的文件，重新加载其内容
                if (SelectedLogFile != null) await LoadLogsAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"自动刷新失败: {ex.Message}");
            StatusText = $"自动刷新失败: {ex.Message}";
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    /// <summary>
    ///     加载今天的日志文件
    /// </summary>
    [RelayCommand]
    private async Task LoadTodayLogsAsync()
    {
        try
        {
            if (_logReaderService.HasTodayLogFile())
            {
                var todayFilePath = _logReaderService.GetTodayLogFilePath();
                var todayFileInfo = _logReaderService.GetLogFileInfo(todayFilePath);

                if (todayFileInfo != null)
                {
                    SelectedLogFile = todayFileInfo;
                    await LoadLogsAsync();
                    StatusText = "已加载今天的日志文件";
                }
            }
            else
            {
                StatusText = "今天还没有日志文件";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"加载今天日志失败: {ex.Message}";
        }
    }

    /// <summary>
    ///     导出日志到文件
    /// </summary>
    [RelayCommand]
    private async Task ExportLogsAsync()
    {
        if (FilteredLogEntries.Count == 0)
        {
            StatusText = "没有日志可以导出";
            return;
        }

        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "文本文件 (*.txt)|*.txt|CSV文件 (*.csv)|*.csv",
                DefaultExt = "txt",
                FileName = $"日志导出_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await Task.Run(() =>
                {
                    var lines = new List<string>();

                    if (saveFileDialog.FilterIndex == 1) // TXT格式
                    {
                        lines.Add("日志导出报告");
                        lines.Add($"导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        lines.Add($"总条目数: {FilteredLogEntries.Count}");
                        lines.Add(new string('-', 80));

                        foreach (var entry in FilteredLogEntries)
                        {
                            lines.Add($"[{entry.FormattedTimestamp}] [{entry.LevelText}] {entry.Message}");
                            if (!string.IsNullOrEmpty(entry.Exception)) lines.Add($"异常信息: {entry.Exception}");

                            lines.Add("");
                        }
                    }
                    else // CSV格式
                    {
                        lines.Add("时间,等级,消息,异常信息,来源");
                        foreach (var entry in FilteredLogEntries)
                        {
                            var message = entry.Message.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", " ");
                            var exception =
                                entry.Exception?.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", " ") ?? "";
                            lines.Add(
                                $"\"{entry.FormattedTimestamp}\",\"{entry.LevelText}\",\"{message}\",\"{exception}\",\"{entry.Source}\"");
                        }
                    }

                    File.WriteAllLines(saveFileDialog.FileName, lines);
                });

                StatusText = $"已导出 {FilteredLogEntries.Count} 条日志到: {saveFileDialog.FileName}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"导出日志失败: {ex.Message}";
            MessageBox.Show($"导出日志失败: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}