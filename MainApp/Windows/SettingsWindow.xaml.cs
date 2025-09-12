using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Core;
using Core.Interfaces;
using Core.Services;
using Core.Utils;
using MainApp.ViewModels;
using MainApp.Views.Settings;
using Microsoft.Win32;
using UI.Controls;
using MessageBox = UI.Controls.MessageBox;

namespace MainApp.Windows;

/// <summary>
///     SettingsWindow.xaml 的交互逻辑
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly IConfigManager _configManager;
    private readonly SettingsControlFactory _settingsFactory;
    private string _currentCategory = "System";
    private ISettingsControl? _currentSettingsControl;

	public SettingsWindow()
    {
        InitializeComponent();
        // 启用窗口拖动
        MouseLeftButtonDown += (s, e) => DragMove();
        _configManager = ConfigManager.Instance;
        _settingsFactory = new SettingsControlFactory();

        // 订阅配置变更事件
        _configManager.ConfigChanged += OnConfigChanged;
		InitializeModuleSettings();

		// 默认显示通用设置
		ShowSettingsCategory(_currentCategory);

	}

    /// <summary>
    ///     配置变更事件处理
    /// </summary>
    private void OnConfigChanged(object? sender, ConfigChangedEventArgs e)
    {
        // 在UI线程中刷新当前显示的设置控件
        Dispatcher.Invoke(async () =>
        {
            // 根据配置类型和当前显示的分类决定是否刷新
            var shouldRefresh = e.ConfigType switch
            {
                ConfigType.System => _currentCategory == "System",
                ConfigType.Database => _currentCategory == "Database",
                ConfigType.Plc => _currentCategory == "Plc",
                _ => false
            };

            if (shouldRefresh && _currentSettingsControl != null) await _currentSettingsControl.RefreshAsync();
        });
    }

	/// <summary>
	/// 初始化模块设置
	/// </summary>
	private void InitializeModuleSettings()
	{
        ModuleManager manager = ServiceLocator.GetService<ModuleManager>();
        if (manager == null) return;
        if (manager.Modules.Count == 0) return;

		//ModuleSettingsTitle.Visibility = Visibility.Collapsed;
		ModuleSettingsPanel.Children.Clear();

		//ModuleSettingsTitle.Visibility = Visibility.Visible;

		foreach (var moduleItem in manager.Modules)
		{
            
			var button = new Button
			{
				Content = moduleItem.ModuleName,
				Height = 35,
				Margin = new Thickness(0, 0, 0, 5),
				Tag = moduleItem.ModuleId,
				HorizontalAlignment = HorizontalAlignment.Stretch
			};

			button.Click += (s, e) =>
			{
				if (s is Button btn && btn.Tag is string moduleId)
				{
					ShowModuleSettings(moduleItem);
				}
			};

			ModuleSettingsPanel.Children.Add(button);
		}
	}

	/// <summary>
	/// 显示模块设置
	/// </summary>
	private void ShowModuleSettings(IModule module)
	{
		if (module.HasSettingsPage)
		{
			SettingsContentControl.Content = module.GetSettingsPage();
		}
		else
		{
			ShowErrorMessage("无法加载模块设置页面");
		}
	}

	/// <summary>
	///     设置分类按钮点击事件
	/// </summary>
	private void SettingsCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string category)
        {
            _currentCategory = category;
            ShowSettingsCategory(category);
        }
    }

    /// <summary>
    ///     显示错误消息
    /// </summary>
    private void ShowErrorMessage(string message)
    {
        var panel = new StackPanel();
        panel.Children.Add(new TextBlock
        {
            Text = message,
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 50, 0, 0),
            Foreground = Brushes.Red,
            TextWrapping = TextWrapping.Wrap
        });
        SettingsContentControl.Content = panel;
    }

    /// <summary>
    ///     显示设置分类
    /// </summary>
    private async void ShowSettingsCategory(string category)
    {
        try
        {
            // 获取设置控件
            var settingsControl = _settingsFactory.GetControl(category);
            if (settingsControl == null)
            {
                ShowErrorMessage($"无法加载设置分类: {category}");
                return;
            }

            // 订阅设置更改事件
            if (_currentSettingsControl != null) _currentSettingsControl.SettingsChanged -= OnSettingsControlChanged;

            _currentSettingsControl = settingsControl;
            _currentSettingsControl.SettingsChanged += OnSettingsControlChanged;

            // 更新标题
            SettingsTitleTextBlock.Text = settingsControl.DisplayTitle;

            // 初始化并加载设置数据
            await settingsControl.InitializeAsync(_configManager);
            await settingsControl.LoadSettingsAsync();

            // 显示控件
            SettingsContentControl.Content = settingsControl;

            // 更新未保存更改指示器
            UpdateUnsavedChangesIndicator();
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"加载设置分类失败: {ex.Message}");
        }
    }

    /// <summary>
    ///     关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    ///     窗口关闭时释放资源
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        // 取消订阅配置变更事件
        _configManager.ConfigChanged -= OnConfigChanged;

        // 取消订阅设置控件事件
        if (_currentSettingsControl != null) _currentSettingsControl.SettingsChanged -= OnSettingsControlChanged;

        base.OnClosed(e);
    }

    /// <summary>
    ///     设置控件更改事件处理
    /// </summary>
    private void OnSettingsControlChanged(object? sender, SettingsChangedEventArgs e)
    {
        UpdateUnsavedChangesIndicator();
    }

    /// <summary>
    ///     更新未保存更改指示器
    /// </summary>
    private void UpdateUnsavedChangesIndicator()
    {
        var hasUnsavedChanges = _settingsFactory.HasUnsavedChanges();
        UnsavedChangesIndicator.Visibility = hasUnsavedChanges ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        // 打开配置文件目录
        var path = Constants.ConfigRootPath;
        if (Directory.Exists(path)) Process.Start("explorer.exe", path);
    }

    private void LogButton_Click(object sender, RoutedEventArgs e)
    {
        ShellWindow logWindow = new("日志查看器", "\ueb6a");
        logWindow.Width = 1200;
        logWindow.Height = 800;
        LogPageViewModel lvm = new();
        logWindow.SetContent(new LogPage(lvm));
        logWindow.Show();
    }

    #region 按钮事件处理

    /// <summary>
    ///     刷新按钮点击事件
    /// </summary>
    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 刷新配置缓存
            await _configManager.RefreshCacheAsync();

            // 刷新当前设置控件
            if (_currentSettingsControl != null) await _currentSettingsControl.RefreshAsync();

            UpdateUnsavedChangesIndicator();
        }
        catch (Exception ex)
        {
            MessageBox.Error($"刷新设置失败: {ex.Message}");
        }
    }

    /// <summary>
    ///     保存按钮点击事件
    /// </summary>
    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveSettingsAsync(true);
    }

    /// <summary>
    ///     应用按钮点击事件
    /// </summary>
    private async void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        await SaveSettingsAsync(false);
    }

    /// <summary>
    ///     重置按钮点击事件
    /// </summary>
    private async void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Question("确定要重置当前设置到默认值吗？", "确认重置");

        if (result == MessageBoxResult.Yes && _currentSettingsControl != null)
            try
            {
                await _currentSettingsControl.ResetToDefaultAsync();
                UpdateUnsavedChangesIndicator();
            }
            catch (Exception ex)
            {
                MessageBox.Error($"重置设置失败: {ex.Message}");
            }
    }

    #endregion

    #region 辅助方法

    /// <summary>
    ///     保存设置
    /// </summary>
    private async Task SaveSettingsAsync(bool showSuccessMessage)
    {
        try
        {
            // 验证所有设置
            var validationResult = _settingsFactory.ValidateAllControls();
            if (!validationResult.IsValid)
            {
                var errors = string.Join(Environment.NewLine, validationResult.Errors);
                MessageBox.Error($"配置验证失败:{Environment.NewLine}{errors}");
                return;
            }

            // 保存所有设置控件的数据
            foreach (var control in _settingsFactory.GetControlsWithUnsavedChanges()) await control.SaveSettingsAsync();

            // 应用开机自启动设置
            var systemSettings = await _configManager.LoadSystemConfigAsync();
            ApplyAutoStartupSetting(systemSettings.IsUseAutoStart);

            UpdateUnsavedChangesIndicator();

            if (showSuccessMessage) MessageBox.Success("设置已保存");
        }
        catch (Exception ex)
        {
            MessageBox.Error($"保存设置失败: {ex.Message}");
        }
    }

    /// <summary>
    ///     应用开机自启动设置
    /// </summary>
    private void ApplyAutoStartupSetting(bool autoStartup)
    {
        try
        {
            const string keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            const string appName = "WinmTech-Scada";

            using var key = Registry.CurrentUser.OpenSubKey(keyName, true);
            if (key != null)
            {
                if (autoStartup)
                {
                    var exePath = Assembly.GetExecutingAssembly().Location;
                    key.SetValue(appName, exePath);
                }
                else
                {
                    key.DeleteValue(appName, false);
                }
            }
        }
        catch (Exception ex)
        {
            // 记录错误但不影响主要功能
            Debug.WriteLine($"设置开机自启动失败: {ex.Message}");
        }
    }

    #endregion
}