using Core.Events;
using Core.Localization;
using Core.Services;
using HandyControl.Tools;
using Logger;
using MainApp.Menu;
using MainApp.ViewModels;
using MainApp.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI.Controls;
using MessageBox = UI.Controls.MessageBox;

namespace MainApp;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static DateTime? lastSuccessTime;
    private readonly EventSubscriber _eventSubscriber;

    private readonly MenuManager _menuManager;
    private readonly MainWindowViewModel _viewModel;
    private readonly ViewsManager _viewsManager;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel();
        Title = _viewModel.Title;
        DataContext = _viewModel;
        _viewsManager = new ViewsManager(MainContentControl);
        _menuManager = new MenuManager(this, MenuPanel, _viewsManager);
        InitMenus();
    }

    private void InitMenus()
    {
        _menuManager.AddMenus(MenuDefinition.Menus.ToList());
        _menuManager.Apply(this, MenuPanel, _viewsManager);
    }


    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
        // Create a new instance of the AboutWindow
        AboutWindow aboutWindow = new();
        // Show the AboutWindow
        aboutWindow.ShowDialog();
    }

    /// <summary>
    ///     设置按钮点击事件
    /// </summary>
    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        SettingsWindow settingsView = new();

        const int timeoutMinutes = 60; // 设置超时时间为60分钟

//#if DEBUG
        // 如果是调试模式，直接打开设置窗体
        settingsView.ShowDialog();
        return;
//#endif

        // 检查是否在有效时间内
        if (lastSuccessTime.HasValue &&
            DateTime.Now.Subtract(lastSuccessTime.Value).TotalMinutes < timeoutMinutes)
        {
            settingsView.ShowDialog();
            return;
        }

        var (success, val) = InputDialog.ShowPasswordInput(LocalizationProvider.Default["Tip"], LocalizationProvider.Default["Tip_EnterPassword"]);
        if (success)
        {
            if (val != "winm666")
            {
                MessageBox.Show(LocalizationProvider.Default["Tip_IncorrectPassword"], LocalizationProvider.Default["Tip"]);
                return;
            }

            // 密码正确，记录时间并显示设置窗体
            lastSuccessTime = DateTime.Now;
            settingsView.ShowDialog();
        }
    }

	private void LangButton_Click(object sender, RoutedEventArgs e) => PopupConfig.IsOpen = true;


	private void ButtonLangs_OnClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Button { Tag: string langName })
        {
			PopupConfig.IsOpen = false;
			ConfigHelper.Instance.SetLang(langName);
			App.SwitchLanguage(langName);
		}
			
        //var current = Thread.CurrentThread.CurrentCulture.IetfLanguageTag;
        //if (current == "zh-CN")
        //{
        //    ConfigHelper.Instance.SetLang("fr");
        //    App.SwitchLanguage("fr-FR");
        //}
        //else
        //{
        //    ConfigHelper.Instance.SetLang("zh");
        //    App.SwitchLanguage("zh-CN");
        //}
    }

	/// <summary>
	///     最小化按钮点击事件
	/// </summary>
	private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    ///     最大化/还原按钮点击事件
    /// </summary>
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    /// <summary>
    ///     关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        var res = MessageBox.Question(LocalizationProvider.Default["Tip_CloseApp"]);
        if (res == MessageBoxResult.No)
            return;
		Close();
    }

    /// <summary>
    ///     窗口关闭事件
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        try
        {
            // 清理磁盘监控资源
            if (_viewModel?.DiskMonitorViewModel != null)
            {
                _viewModel.DiskMonitorViewModel.StopMonitoring();
                _viewModel.DiskMonitorViewModel.Dispose();
            }

            // 清理其他资源
            _viewModel?.Dispose();
        }
        catch (Exception ex)
        {
            Log.Error("窗口关闭时清理资源失败", ex);
        }
        finally
        {
            base.OnClosed(e);
        }
    }

    /// <summary>
    ///     窗体状态改变事件
    /// </summary>
    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            MaximizeRestoreButton.Content = "🗗";
        else
            MaximizeRestoreButton.Content = "🗖";
    }

    /// <summary>
    ///     标题栏拖拽事件
    /// </summary>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            MaximizeButton_Click(sender, e);
        else
            DragMove();
    }

}