using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace Module.Business.SG141.ViewModels;

public partial class SG141SettingsViewModel: ObservableObject
{
	[ObservableProperty] private ObservableCollection<DeviceModel> _devices;

	[ObservableProperty] private int _dailyPlanCount = 0;
	[ObservableProperty] private int _planSpeed = 0;

	public SG141SettingsViewModel()
	{
		Devices = new ObservableCollection<DeviceModel>
		{
			new DeviceModel { DeviceName = "设备1", IsActive = true, TargetPlc = "PLC001", PlcNode = "Node001" },
			new DeviceModel { DeviceName = "设备2", IsActive = false, TargetPlc = "PLC002", PlcNode = "Node002" }
		};
	}

	// 添加设备命令
	[RelayCommand]
	private void AddDevice()
	{
		Devices.Add(new DeviceModel
		{
			DeviceName = $"设备{Devices.Count + 1}",
			IsActive = true,
			TargetPlc = "PLC_New",
			PlcNode = "Node_New"
		});
	}

	// 保存命令
	[RelayCommand]
	private void Save()
	{
		MessageBox.Show("设备列表已保存！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
	}
}


public partial class DeviceModel : ObservableObject
{
	[ObservableProperty]
	private string _deviceName;

	[ObservableProperty]
	private bool _isActive;

	[ObservableProperty]
	private string _targetPlc;

	[ObservableProperty]
	private string _plcNode;
}
