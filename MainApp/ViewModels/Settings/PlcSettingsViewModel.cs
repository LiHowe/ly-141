using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Connection.S7;
using Core;
using Core.Models.Settings;
using Core.Utils;
using UI.Converters;
using UI.ViewModels;

namespace MainApp.ViewModels.Settings;

public partial class PlcSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<S7PlcViewModel> _configs = new();
    [ObservableProperty] private PlcSettings _settings = new();

    public void LoadSettings(PlcSettings settings)
    {
        _settings = settings;
        Configs.Clear();
        var data = settings.Configs
            .Select(S7PlcConfigConverter.ToViewModel)
            .Select(HandleNodesChanged)
            .Select(HandlePlcRemoveRequested)
            .ToList();
        Configs = new ObservableCollection<S7PlcViewModel>(data);
    }

    private S7PlcViewModel HandleNodesChanged(S7PlcViewModel vm)
    {
        vm.SettingsChanged += (s, e) => { OnSettingsChanged(); };
        return vm;
    }

    private S7PlcViewModel HandlePlcRemoveRequested(S7PlcViewModel vm)
    {
        vm.DeviceRemoveRequested += (s, e) =>
        {
            Configs.Remove(vm);
            OnSettingsChanged();
        };
        return vm;
    }


    public async Task SaveSettings()
    {
        if (Settings == null) return;

        var data = Configs.Select(S7PlcConfigConverter.ToConfig).ToList();
        ObservableCollection<S7PlcConfig> configs = new(data);

        Settings.Configs = configs;
        await ConfigManager.Instance.SaveConfigAsync(Constants.PlcConfigFilePath, Settings);
    }

    public List<string> ValidateSettings()
    {
        var errors = new List<string>();
        // 同步配置
        Settings.Configs = new ObservableCollection<S7PlcConfig>(
            Configs.Select(S7PlcConfigConverter.ToConfig));

        if (Settings.Configs == null || Settings.Configs.Count == 0)
            errors.Add("至少需要配置一个PLC");

        if (Settings.Configs.GroupBy(x => x.Key).Any(x => x.Count() > 1)) errors.Add("PLC标识不能重复");

        if (Settings.Configs.Any(x => x.Nodes.Count == 0)) errors.Add("PLC节点数量不能为0");

        var a = Settings.Configs.Select(x =>
                {
                    var keys = x.Nodes
                        .GroupBy(y => y.Title)
                        .Where(x => x.Count() > 1)
                        .Select(x => x.Key)
                        .ToList();
                    return (x.Key, Keys: keys);
                }
            )
            .Where(x => x.Keys.Count > 0).ToList();

        // 检查每个配置是否有重复节点配置
        if (a.Any())
        {
            StringBuilder builder = new();
            builder.AppendLine("PLC节点名称不能重复!");
            foreach ((string Key, List<string> Keys) tuple in a)
                builder.AppendLine($"PLC:{tuple.Key} 重复节点:{string.Join(",", tuple.Keys)}");
            errors.Add(builder.ToString());
        }

        return errors;
    }

    public event EventHandler? SettingsChanged;

    private void OnSettingsChanged()
    {
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnConfigsChanged(ObservableCollection<S7PlcViewModel> value)
    {
        OnSettingsChanged();
    }

    [RelayCommand]
    private void AddPlc()
    {
        var plc = new S7PlcViewModel();
        Configs.Add(plc);
        OnSettingsChanged();
    }

    [RelayCommand]
    private void RemovePlc(S7PlcViewModel config)
    {
        Configs.Remove(config);
        OnSettingsChanged();
    }

    [RelayCommand]
    private void TestPlcConnection(S7PlcViewModel config)
    {
    }

    [RelayCommand]
    private void TestAllConnection()
    {
    }
}