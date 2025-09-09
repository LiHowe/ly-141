using System.Windows.Input;
using Core.Services;
using Core.Utils;
using Logger;
using MainApp.ViewModels.Settings;
using UI.Controls;
using ValidationResult = Core.Interfaces.ValidationResult;

namespace MainApp.Views.Settings;

public partial class SystemSettingsControl : BaseSettingsControl
{
    private readonly SystemSettingsViewModel _viewModel;

    public SystemSettingsControl()
    {
        InitializeComponent();
        CategoryName = "SysConfig";
        DisplayTitle = "系统设置";
        _viewModel = new SystemSettingsViewModel();
        DataContext = _viewModel;
        _viewModel.SettingsChanged += OnViewModelSettingsChanged;
        KeyDown += (s, e) =>
        {
            // 当按下F11的时候
            if (e.Key == Key.F11)
                // 开启/关闭开发者模式
                _viewModel.IsShowDevPanel = !_viewModel.IsShowDevPanel;
        };
    }

    public override string CategoryName { get; }
    public override string DisplayTitle { get; }

    protected override async Task OnLoadSettingsAsync()
    {
        try
        {
            if (_configManager == null) return;

            var systemSettings = await _configManager.LoadSystemConfigAsync();
            await UIThreadHelper.InvokeAsync(() =>
            {
                // 在UI线程中更新UI
                _viewModel?.LoadSettings(systemSettings);
                HasUnsavedChanges = false;
            });
        }
        catch (Exception ex)
        {
            MessageBox.Error($"加载系统设置失败: {ex.Message}");
        }
    }

    protected override async Task OnSaveSettingsAsync()
    {
        try
        {
            await _viewModel?.SaveSettings();
        }
        catch (Exception ex)
        {
            Log.Error("保存系统设置失败", ex);
            MessageBox.Error("保存系统设置失败: " + ex.Message);
        }
    }

    protected override ValidationResult OnValidateSettings()
    {
        return new ValidationResult();
    }

    protected override Task OnResetToDefaultAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     视图模型设置更改事件处理
    /// </summary>
    private void OnViewModelSettingsChanged(object? sender, EventArgs e)
    {
        HasUnsavedChanges = true;
        OnSettingsChanged("ViewModel", null, null);
    }
}