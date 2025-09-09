
using Core.Services;
using Core.Utils;
using Logger;
using MainApp.ViewModels.Settings;
using UI.Controls;
using ValidationResult = Core.Interfaces.ValidationResult;

namespace MainApp.Views.Settings;

public partial class DatabaseSettingsControl : BaseSettingsControl
{
    private readonly DatabaseSettingsViewModel _viewModel;

    public DatabaseSettingsControl()
    {
        InitializeComponent();
        CategoryName = "Database";
        DisplayTitle = "数据库设置";
        _viewModel = new DatabaseSettingsViewModel();
        DataContext = _viewModel;
        _viewModel.SettingsChanged += OnViewModelSettingsChanged;
    }

    public override string CategoryName { get; }
    public override string DisplayTitle { get; }

    protected override async Task OnLoadSettingsAsync()
    {
        try
        {
            if (_configManager == null) return;

            var databaseSettings = await _configManager.LoadDbConfigAsync();
            await UIThreadHelper.InvokeAsync(() =>
            {
                // 在UI线程中更新UI
                _viewModel?.LoadSettings(databaseSettings);
                HasUnsavedChanges = false;
            });
        }
        catch (Exception ex)
        {
            MessageBox.Error($"加载数据库设置失败: {ex.Message}");
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
            Log.Error("保存数据库设置失败", ex);
            MessageBox.Error("保存数据库设置失败: " + ex.Message);
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