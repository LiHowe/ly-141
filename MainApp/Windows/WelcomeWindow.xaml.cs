using System.Windows;
using System.Windows.Threading;
using Core;
using Core.Models.Settings;
using Core.Utils;
using static UI.Controls.MessageBox;
using MessageBox = UI.Controls.MessageBox;

namespace MainApp.Windows;

/// <summary>
///     WelcomeWindow.xaml 的交互逻辑，显示加载界面并管理加载任务，加载完成后显示 MainWindow
/// </summary>
public partial class WelcomeWindow : Window
{
    // 取消加载的令牌源
    private readonly CancellationTokenSource _cancellationTokenSource;

    // 标记是否并行执行任务
    private readonly bool _isParallelExecution;

    // 存储加载任务的列表，每个任务包含名称和执行方法
    private readonly List<(string Name, Func<Task<bool>> Action)> _loadingTasks = new();

    /// <summary>
    ///     构造函数，初始化窗口和加载设置
    /// </summary>
    /// <param name="isParallelExecution">是否并行执行任务</param>
    public WelcomeWindow(bool isParallelExecution = false)
    {
        InitializeComponent();
        // 加载系统配置
        var sysConfig = ConfigManager.Instance.LoadConfig<SystemSettings>(Constants.SystemConfigFilePath);

        // 设置初始文本
        SystemTitleTextBlock.Text = sysConfig?.SystemName ?? "维美数据采集与追溯系统";
        SystemSubtitleTextBlock.Text = sysConfig?.SystemSubName ?? "WinM Data Acquisition and Traceability System";
        SystemVersionTextBlock.Text = sysConfig?.SystemVersion ?? "3.0.0";
        LoadingStatusTextBlock.Text = "正在初始化...";

        // 记录是否并行执行
        _isParallelExecution = isParallelExecution;
        // 初始化取消令牌
        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    ///     注册新的加载任务
    /// </summary>
    /// <param name="taskName">任务名称</param>
    /// <param name="taskAction">任务执行方法，返回 true 表示成功，false 表示失败</param>
    public void RegisterLoadingTask(string taskName, Func<Task<bool>> taskAction)
    {
        _loadingTasks.Add((taskName, taskAction));
    }

    /// <summary>
    ///     开始执行所有注册的加载任务，并在成功完成后显示 MainWindow
    /// </summary>
    public async Task StartLoadingAsync()
    {
        try
        {
            // 设置进度条最大值
            LoadingProgressBar.Maximum = _loadingTasks.Count;
            LoadingProgressBar.Value = 0;

            bool loadSuccessful;
            if (_isParallelExecution)
                // 并行执行所有任务
                loadSuccessful = await ExecuteTasksParallelAsync();
            else
                // 顺序执行所有任务
                loadSuccessful = await ExecuteTasksSequentialAsync();

            if (loadSuccessful)
            {
                // 加载完成，更新状态
                UpdateLoadingStatus("加载完成");
                LoadingProgressBar.Value = _loadingTasks.Count;

                // 延迟1秒后显示 MainWindow 并关闭 WelcomeWindow
                //await Task.Delay(1000, _cancellationTokenSource.Token);
                ShowMainWindow();
            }
            else
            {
                // 加载失败或用户选择退出
                Application.Current.Shutdown();
            }
        }
        catch (OperationCanceledException)
        {
            // 用户取消加载
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            // 捕获未处理的异常
            UpdateLoadingStatus("发生错误");
            ShowErrorMessage($"加载失败: {ex.Message}", false);
            Application.Current.Shutdown();
        }
    }

    /// <summary>
    ///     显示 MainWindow 并关闭当前窗口
    /// </summary>
    private void ShowMainWindow()
    {
        Dispatcher.Invoke(() =>
        {
            var closeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(800)
            };
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                // 创建 MainWindow 实例
                var mainWindow = new MainWindow();
                // 显示主窗口
                mainWindow.Show();
                // 设置为应用程序主窗口
                Application.Current.MainWindow = mainWindow;
                Close();
            };
            closeTimer.Start();
        });
        //// 关闭加载窗口
        //Close();
    }

    /// <summary>
    ///     并行执行所有任务
    /// </summary>
    /// <returns>返回 true 表示所有任务成功或用户选择继续，false 表示用户选择退出</returns>
    private async Task<bool> ExecuteTasksParallelAsync()
    {
        var tasks = new List<Task<bool>>();
        foreach (var task in _loadingTasks)
        {
            UpdateLoadingStatus($"正在加载: {task.Name}");
            tasks.Add(ExecuteTaskWithRetryAsync(task));
        }

        // 等待所有任务完成
        var results = await Task.WhenAll(tasks);

        // 更新进度
        LoadingProgressBar.Value = _loadingTasks.Count;

        // 检查是否有任务失败
        for (var i = 0; i < results.Length; i++)
            if (!results[i])
            {
                var continueExecution = await HandleTaskFailureAsync(_loadingTasks[i].Name);
                if (!continueExecution) return false; // 用户选择退出
            }

        return true;
    }

    /// <summary>
    ///     顺序执行所有任务
    /// </summary>
    /// <returns>返回 true 表示所有任务成功或用户选择继续，false 表示用户选择退出</returns>
    private async Task<bool> ExecuteTasksSequentialAsync()
    {
        for (var i = 0; i < _loadingTasks.Count; i++)
        {
            var task = _loadingTasks[i];
            UpdateLoadingStatus($"正在加载: {task.Name}");

            var success = await ExecuteTaskWithRetryAsync(task);
            LoadingProgressBar.Value = i + 1; // 更新进度

            if (!success) return false; // 用户选择退出
        }

        return true;
    }

    /// <summary>
    ///     执行单个任务并处理重试逻辑
    /// </summary>
    /// <param name="task">任务信息（名称和执行方法）</param>
    /// <returns>返回 true 表示任务成功或继续，false 表示退出</returns>
    private async Task<bool> ExecuteTaskWithRetryAsync((string Name, Func<Task<bool>> Action) task)
    {
        while (true)
            try
            {
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                var success = await task.Action();
                if (success) return true; // 任务成功

                // 任务失败，弹出提示
                var continueExecution = await ShowTaskFailureDialogAsync(task.Name);
                if (continueExecution == true) continue; // 重试

                if (continueExecution == false) return false; // 退出
                return true; // 继续下一个任务
            }
            catch (OperationCanceledException)
            {
                throw; // 取消操作，退出
            }
            catch (Exception ex)
            {
                var continueExecution = await ShowTaskFailureDialogAsync(task.Name, ex.Message);
                if (continueExecution == true) continue; // 重试

                if (continueExecution == false) return false; // 退出
                return true; // 继续下一个任务
            }
    }

    /// <summary>
    ///     显示任务失败的提示框
    /// </summary>
    /// <param name="taskName">失败的任务名称</param>
    /// <param name="errorMessage">错误详细信息（可选）</param>
    /// <returns>返回 true 表示重试，false 表示退出，null 表示继续</returns>
    // 修改 ShowTaskFailureDialogAsync 方法
    private async Task<bool?> ShowTaskFailureDialogAsync(string taskName, string errorMessage = null)
    {
        var buttons = new[]
        {
            ("继续", MessageBoxResult.None, false),
            ("重试", MessageBoxResult.Yes, false),
            ("退出", MessageBoxResult.No, false)
        };
        var result = ShowCustom(Owner,
            $"任务 '{taskName}' 加载失败{(errorMessage != null ? $": {errorMessage}" : "")}",
            "加载错误", MessageBoxType.Error, buttons);
        return result switch
        {
            MessageBoxResult.Yes => true, // 重试
            MessageBoxResult.No => false, // 退出
            _ => null // 继续
        };
    }

    /// <summary>
    ///     显示错误消息（不带重试选项）
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="canContinue">是否允许继续</param>
    private void ShowErrorMessage(string message, bool canContinue)
    {
        var buttons = canContinue ? MessageBoxButtons.OK : MessageBoxButtons.OKCancel;
        MessageBox.Show(this, message, "错误", MessageBoxType.Error, buttons);
    }

    /// <summary>
    ///     关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // 取消加载并退出程序
        _cancellationTokenSource.Cancel();
        Application.Current.Shutdown();
    }

    /// <summary>
    ///     更新加载状态，线程安全
    /// </summary>
    /// <param name="status">加载状态文本</param>
    public void UpdateLoadingStatus(string status)
    {
        Dispatcher.Invoke(() => LoadingStatusTextBlock.Text = status);
    }

    /// <summary>
    ///     清理资源
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        base.OnClosed(e);
    }

    /// <summary>
    ///     处理任务失败（仅用于并行执行）
    /// </summary>
    /// <param name="taskName">失败的任务名称</param>
    /// <returns>返回 true 表示继续，false 表示退出</returns>
    private async Task<bool> HandleTaskFailureAsync(string taskName)
    {
        var result = await ShowTaskFailureDialogAsync(taskName);
        return result != false; // 返回 false 表示退出
    }
}