
using Core.Services;
using Core.Utils;
using Logger;
using MainApp.ViewModels.Settings;
using UI.Controls;
using ValidationResult = Core.Interfaces.ValidationResult;

namespace MainApp.Views.Settings;

public partial class PlcSettingsControl : BaseSettingsControl
{
    private readonly PlcSettingsViewModel _viewModel;

    public PlcSettingsControl()
    {
        InitializeComponent();
        _viewModel = new PlcSettingsViewModel();
        DataContext = _viewModel;
        _viewModel.SettingsChanged += OnViewModelSettingsChanged;
        CategoryName = "Plc";
        DisplayTitle = "PLC设置";
    }

    public override string CategoryName { get; }
    public override string DisplayTitle { get; }

    protected override async Task OnLoadSettingsAsync()
    {
        try
        {
            if (_configManager == null) return;

            var plcSettings = await _configManager.LoadPlcConfigAsync();
            await UIThreadHelper.InvokeAsync(() =>
            {
                _viewModel?.LoadSettings(plcSettings);
                HasUnsavedChanges = false;
            });
        }
        catch (Exception ex)
        {
            MessageBox.Error($"加载PLC设置失败: {ex.Message}");
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
            Log.Error("保存PLC设置失败", ex);
            MessageBox.Error("保存PLC设置失败: " + ex.Message);
        }
    }

    protected override ValidationResult OnValidateSettings()
    {
        var result = new ValidationResult();

        if (_viewModel != null)
        {
            var errors = _viewModel.ValidateSettings();
            foreach (var error in errors) result.AddError(error);
        }

        return result;
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